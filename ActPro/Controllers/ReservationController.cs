using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        //--- RESERVATION PAGE ---
        public async Task<IActionResult> Index(int id)
        {
            var place = await _context.Places
                .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                .Include(p => p.PlaceImages)
                .Include(p => p.City)
                .Include(p => p.Activity)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (place == null)
            {
                return NotFound();
            }
            var userId = _userManager.GetUserId(User);
            var viewModel = new ReservationViewModel
            {
                Place = place,
                IsFavorite = userId != null && await _context.Favorites.AnyAsync(f => f.PlaceId == id && f.AspNetUserId == userId)
            };

            return View("Reservation", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(int placeId, DateTime date, string timeSlot)
        {
            return RedirectToAction("Confirmation", new { id = placeId });
        }
        public IActionResult Confirmation(int id)
        {
            ViewBag.PlaceId = id;
            return View();
        }

        //--- ADD REVIEW ---
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int placeId, string commentText, int rating)
        {
            var userId = _userManager.GetUserId(User);
            int userCommentCount = await _context.Comments
            .CountAsync(c => c.PlaceId == placeId && c.AspNetUserId == userId);

            if (userCommentCount >= 3)
            {
                TempData["Error"] = "Вече сте оставили максималния брой коментари (3) за този обект.";
                return RedirectToAction("Index", new { id = placeId });
            }

            var newComment = new Comment
            {
                PlaceId = placeId,
                AspNetUserId = userId,
                CommentText = commentText,
                Rating = rating,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            var place = await _context.Places.Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == placeId);
            if (place != null)
            {
                var allRatings = place.Comments.Select(c => (double)c.Rating).ToList();
                allRatings.Add(rating);
                place.Rating = (int)Math.Round(allRatings.Average());
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { id = placeId });
        }

        //--- DELETE REVIEW ---
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int reviewId, int placeId)
        {
            var userId = _userManager.GetUserId(User);
            var comment = await _context.Comments
           .FirstOrDefaultAsync(c => c.Id == reviewId && c.AspNetUserId == userId);

            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                var place = await _context.Places
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == placeId);

                if (place != null)
                {
                    if (place.Comments.Any())
                    {
                        place.Rating = (int)Math.Round(place.Comments.Average(c => (double)c.Rating));
                    }
                    else
                    {
                        place.Rating = 0;
                    }

                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index", new { id = placeId });
        }
    }
}