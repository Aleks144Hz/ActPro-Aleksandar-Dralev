using ActPro.DAL;
using ActPro.Domain;
using ActPro.Services;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ActPro.Controllers
{
    public class ReservationController(IReservationService reservationService, UserManager<ApplicationUser> userManager, IAuditService auditService, IEmailSender emailSender) : Controller
    {
        //--- RESERVATION PAGE ---
        public async Task<IActionResult> Index(int id)
        {
            var userId = userManager.GetUserId(User);
            var viewModel = await reservationService.GetReservationIndexModelAsync(id, userId);
            if (viewModel == null) return NotFound();
            return View("Index", viewModel);
        }

        //--- BOOKING ACTION ---
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int placeId, DateTime date, string timeSlot, string placeName)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            if (!user.EmailConfirmed)
            {
                TempData["Error"] = DomainResources.ReservationNeedApproval;
                return RedirectToAction("Index", new { id = placeId });
            }
            var result = await reservationService.BookAsync(placeId, date, timeSlot, user);

            if (result.success)
            {
                try
                {
                    if (string.IsNullOrEmpty(placeName))
                    {
                        var placeModel = await reservationService.GetReservationIndexModelAsync(placeId, user.Id);
                        placeName = placeModel?.Place?.Name ?? DomainResources.SportPlace;
                    }
                    string formattedDate = date.ToString("dd.MM.yyyy (dddd)");

                    await emailSender.SendBookingConfirmationAsync(
                        user.Email,
                        user.FirstName,
                        placeName,
                        formattedDate,
                        timeSlot
                    );
                }
                catch (Exception ex)
                {

                }
                await auditService.LogAsync("Create Reservation", "User", user.Id, DomainResources.UserMadeReservation);
                return RedirectToAction("Confirmation", new { id = placeId });
            }

            TempData["Error"] = result.message;
            return RedirectToAction("Index", new { id = placeId });
        }

        public IActionResult Confirmation(int id) { ViewBag.PlaceId = id; return View(); }

        //--- CANCEL RESERVATION ---
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = userManager.GetUserId(User);
            var user = await userManager.GetUserAsync(User);
            var reservationsData = await reservationService.GetUserReservationsAsync(userId, 1, 100, "all");
            var reservationToDelete = reservationsData.Reservations.FirstOrDefault(r => r.Id == id);
            var result = await reservationService.CancelReservationAsync(id, userId);

            if (result.success)
            {
                await auditService.LogAsync("Cancel Reservation", "User", userId, DomainResources.UserDeletedReservation);

                if (reservationToDelete != null)
                {
                    try
                    {
                        string formattedDate = reservationToDelete.ReservationDate.ToString();

                        await emailSender.SendBookingCancellationAsync(
                            user.Email,
                            user.FirstName,
                            reservationToDelete.PlaceName,
                            formattedDate,
                            reservationToDelete.ReservationTime.ToString()
                        );
                    }
                    catch { }
                }
            }

            return Json(new { success = result.success, message = result.message });
        }

        //--- ADD REVIEW ---
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int placeId, string commentText, int rating)
        {
            var user = await userManager.GetUserAsync(User);
            var userId = userManager.GetUserId(User);
            if (!user.EmailConfirmed)
            {
                TempData["Error"] = DomainResources.CommentNeedApproval;
                return RedirectToAction("Index", new { id = placeId });
            }
            var result = await reservationService.AddReviewAsync(placeId, userId, commentText, rating);

            if (result.success)
            {
                TempData["Success"] = result.message;
                await auditService.LogAsync("Add Review", "User", userId, DomainResources.UserMadeComment);
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
            var userId = userManager.GetUserId(User);
            var result = await reservationService.EditReviewAsync(id, userId, commentText, rating);
            if (result.success) TempData["Success"] = result.message;
            return RedirectToAction("MyReviews");
        }

        //--- DELETE REVIEW ---
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int reviewId, int placeId)
        {
            var userId = userManager.GetUserId(User);
            var result = await reservationService.DeleteReviewAsync(reviewId, userId);

            if (result.success)
            {
                TempData["Success"] = result.message;
                await auditService.LogAsync("Delete Review", "User", userId, DomainResources.UserDeletedComment);
            }

            if (Request.Headers["Referer"].ToString().Contains("MyReviews")) return RedirectToAction("MyReviews");
            return RedirectToAction("Index", new { id = placeId });
        }

        //--- GET OCCUPIED SLOTS FOR A DATE --- 
        [HttpGet]
        public async Task<JsonResult> GetOccupiedSlots(int placeId, DateTime date) => Json(await reservationService.GetOccupiedSlotsAsync(placeId, date));

        //--- MY RESERVATIONS PAGE ---
        [Authorize]
        public async Task<IActionResult> MyReservations(int page = 1, string filter = "all")
        {
            var userId = userManager.GetUserId(User);
            var viewModel = await reservationService.GetUserReservationsAsync(userId, page, 10, filter);
            return View(viewModel);
        }

        //--- MY REVIEWS PAGE ---
        [Authorize]
        public async Task<IActionResult> MyReviews() => View(await reservationService.GetUserReviewsAsync(userManager.GetUserId(User)));
    }
}