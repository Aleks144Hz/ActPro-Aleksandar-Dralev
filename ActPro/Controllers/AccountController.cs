using ActPro.Models;
using ActPro.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ActPro.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View("Views/Account/Login.cshtml");
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (await _userManager.CheckPasswordAsync(user, model.Password))
                    {
                        await _signInManager.SignInAsync(user, model.RememberMe);
                        //return View(GlobalConstants.ViewPath._Home+"/Index");

                        var userRole = await _userManager.GetRolesAsync(user);

                        if (userRole.FirstOrDefault() == "User")
                        {
                            return RedirectToAction();
                        }

                        return RedirectToAction();
                    }
                    else
                    {
                        // Add a model error if the password is incorrect
                        ModelState.AddModelError(string.Empty, MessageConstants.NotValidPassword);
                        return View();
                    }
                }
                else
                {
                    // Add a model error if the email does not exist
                    ModelState.AddModelError(string.Empty, MessageConstants.UserIsNotRegistered);
                    return View();
                }
            }
            else
            {
                return View();
            }
        }
    }
}
