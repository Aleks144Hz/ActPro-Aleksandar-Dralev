using ActPro.DAL;
using ActPro.Services;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ActPro.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        public ReservationController(IReservationService reservationService, UserManager<ApplicationUser> userManager, IAuditService auditService)
        {
            _reservationService = reservationService;
            _userManager = userManager;
            _auditService = auditService;
        }

        //--- RESERVATION PAGE ---
        public async Task<IActionResult> Index(int id)
        {
            var userId = _userManager.GetUserId(User);
            var viewModel = await _reservationService.GetReservationIndexModelAsync(id, userId);
            if (viewModel == null) return NotFound();
            return View("Index", viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int placeId, DateTime date, string timeSlot)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _reservationService.BookAsync(placeId, date, timeSlot, user);

            if (result.success)
            {
                await _auditService.LogAsync("Create Reservation", "User", user.Id, "Потребителят направи резервация.");
                return RedirectToAction("Confirmation", new { id = placeId });
            }

            TempData["Error"] = result.message;
            return RedirectToAction("Index", new { id = placeId });
        }

        public IActionResult Confirmation(int id) { ViewBag.PlaceId = id; return View(); }

        //--- ADD REVIEW ---
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int placeId, string commentText, int rating)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _reservationService.AddReviewAsync(placeId, userId, commentText, rating);

            if (result.success)
            {
                TempData["Success"] = result.message;
                await _auditService.LogAsync("Add Review", "User", userId, "Потребителят добави коментар.");
            }
            else TempData["Error"] = result.message;

            return RedirectToAction("Index", new { id = placeId });
        }

        //--- EDIT REVIEW ---
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReview(int id, string commentText, int rating)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _reservationService.EditReviewAsync(id, userId, commentText, rating);
            if (result.success) TempData["Success"] = result.message;
            return RedirectToAction("MyReviews");
        }

        //--- DELETE REVIEW ---
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int reviewId, int placeId)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _reservationService.DeleteReviewAsync(reviewId, userId);

            if (result.success)
            {
                TempData["Success"] = result.message;
                await _auditService.LogAsync("Delete Review", "User", userId, "Потребителят изтри коментар.");
            }

            if (Request.Headers["Referer"].ToString().Contains("MyReviews")) return RedirectToAction("MyReviews");
            return RedirectToAction("Index", new { id = placeId });
        }

        //--- GET OCCUPIED SLOTS FOR A DATE --- 
        [HttpGet]
        public async Task<JsonResult> GetOccupiedSlots(int placeId, DateTime date) => Json(await _reservationService.GetOccupiedSlotsAsync(placeId, date));

        //--- MY RESERVATIONS PAGE ---
        [Authorize]
        public async Task<IActionResult> MyReservations(int page = 1, string filter = "all")
        {
            var userId = _userManager.GetUserId(User);

            var viewModel = await _reservationService.GetUserReservationsAsync(userId, page, 10, filter);

            return View(viewModel);
        }

        //--- CANCEL RESERVATION ---
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _reservationService.CancelReservationAsync(id, userId);
            if (result.success) await _auditService.LogAsync("Cancel Reservation", "User", userId, "Потребителят отказа резервация.");
            return Json(new { success = result.success, message = result.message });
        }

        //--- MY REVIEWS PAGE ---
        [Authorize]
        public async Task<IActionResult> MyReviews() => View(await _reservationService.GetUserReviewsAsync(_userManager.GetUserId(User)));
    }
}