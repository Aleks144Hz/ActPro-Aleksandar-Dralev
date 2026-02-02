using ActPro.DAL;
using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Models;
using ActPro.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace ActPro.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        public ReservationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
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

            return View("Index", viewModel);
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(ReservationViewModel model, int placeId, DateTime date, string timeSlot)
        {
            var isClosed = await _context.PlaceClosures.AnyAsync(c =>
             c.PlaceId == placeId &&
             c.ClosureDate.Date == date.Date);

            if (isClosed)
            {
                TempData["Error"] = $"Обектът е затворен за резервации на тази дата.";
                return RedirectToAction("Index", new { id = placeId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            DateOnly parsedDate = DateOnly.FromDateTime(date);
            if (!TimeOnly.TryParse(timeSlot, out TimeOnly parsedTime))
            {
                return RedirectToAction("Index", new { id = placeId });
            }

            var combinedDateTime = parsedDate.ToDateTime(parsedTime);
            if (combinedDateTime < DateTime.Now)
            {
                TempData["Error"] = "Часът вече е минал.";
                return RedirectToAction("Index", new { id = placeId });
            }
            bool isTaken = await _context.Reservations.AnyAsync(r =>
                r.PlaceId == placeId && r.ReservationDate == parsedDate && r.ReservationTime == parsedTime);

            if (isTaken)
            {
                TempData["Error"] = "Този час току-що беше зает!";
                return RedirectToAction("Index", new { id = placeId });
            }
            var reservation = new Reservation
            {
                PlaceId = placeId,
                ReservationDate = parsedDate,
                ReservationTime = parsedTime,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.PhoneNumber,
                AspNetUserId = user.Id,
                CreatedAt = DateTime.Now
            };
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            user.Credits += 3;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Резервацията е успешно направена.";
            await _auditService.LogAsync("Create Reservation", "User", user.Id, $"Потребителят направи резервация.");
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
            var user = await _userManager.GetUserAsync(User);
            user.Credits += 0.5;
            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Коментарът е добавен успешно.";
            await _auditService.LogAsync("Add Review", "User", user.Id, $"Потребителят добави коментар.");
            return RedirectToAction("Index", new { id = placeId });
        }

        //--- EDIT REVIEW ---
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReview(int id, string commentText, int rating)
        {
            var userId = _userManager.GetUserId(User);
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id && c.AspNetUserId == userId);

            if (comment == null) return NotFound();

            comment.CommentText = commentText;
            comment.Rating = rating;
            await _context.SaveChangesAsync();

            var place = await _context.Places.Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == comment.PlaceId);
            if (place != null)
            {
                place.Rating = (int)Math.Round(place.Comments.Average(c => (double)c.Rating));
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Отзивът е актуализиран.";
            return RedirectToAction("MyReviews");
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
                    TempData["Success"] = "Коментарът е изтрит успешно.";
                    await _auditService.LogAsync("Delete Review", "User", userId, $"Потребителят изтри коментар.");
                }
            }
            var user = await _userManager.GetUserAsync(User);
            if (user.Credits >= 0.5)
            {
                user.Credits -= 0.5;
            }
            else
            {
                user.Credits = 0;
            }
            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();
            if (Request.Headers["Referer"].ToString().Contains("MyReviews"))
            {
                return RedirectToAction("MyReviews");
            }
            return RedirectToAction("Index", new { id = placeId });
        }

        //--- GET OCCUPIED SLOTS FOR A DATE ---
        [HttpGet]
        public async Task<JsonResult> GetOccupiedSlots(int placeId, DateTime date)
        {
            var parsedDate = DateOnly.FromDateTime(date);
            var reservations = await _context.Reservations
                .Where(r => r.PlaceId == placeId && r.ReservationDate == parsedDate)
                .Select(r => r.ReservationTime)
                .ToListAsync();
            var occupiedSlots = reservations
                .Where(t => t.HasValue)
                .Select(t => t.Value.ToString("HH:mm"))
                .ToList();
            return Json(occupiedSlots);
        }

        //--- MY RESERVATIONS PAGE ---
        [Authorize]
        public async Task<IActionResult> MyReservations(int page = 1, string filter = "all")
        {
            var userId = _userManager.GetUserId(User);
            int pageSize = 10;
            var query = _context.Reservations
                .Where(r => r.AspNetUserId == userId)
                .AsQueryable();

            var today = DateOnly.FromDateTime(DateTime.Now);
            var now = TimeOnly.FromDateTime(DateTime.Now);

            if (filter == "upcoming")
            {
                query = query.Where(r => r.ReservationDate > today ||
                                        (r.ReservationDate == today && r.ReservationTime > now));
            }
            else if (filter == "past")
            {
                query = query.Where(r => r.ReservationDate < today ||
                                        (r.ReservationDate == today && r.ReservationTime <= now));
            }
            var totalReservations = await query.CountAsync();

            var reservations = await query
                .Include(r => r.Place)
                .ThenInclude(p => p.PlaceImages)
                .Include(r => r.Place)
                .ThenInclude(p => p.City)
                .OrderByDescending(r => r.ReservationDate)
                .ThenByDescending(r => r.ReservationTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalReservations / pageSize);
            ViewBag.TotalCount = totalReservations;
            ViewBag.Filter = filter;

            return View(reservations);
        }

        //--- CANCEL RESERVATION ---
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id && r.AspNetUserId == userId);

            if (reservation == null)
            {
                return Json(new { success = false, message = "Резервацията не е намерена." });
            }

            var reservationDateTime = (reservation.ReservationDate ?? DateOnly.FromDateTime(DateTime.Now))
                                       .ToDateTime(reservation.ReservationTime ?? new TimeOnly(0, 0));

            if (reservationDateTime < DateTime.Now)
            {
                return Json(new { success = false, message = "Не можете да откажете изминала резервация." });
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            await _auditService.LogAsync("Cancel Reservation", "User", userId, $"Потребителят отказа резервация.");

            return Json(new { success = true, message = "Резервацията беше отказана успешно." });
        }

        //--- MY REVIEWS PAGE ---
        [Authorize]
        public async Task<IActionResult> MyReviews()
        {
            var userId = _userManager.GetUserId(User);

            var reviews = await _context.Comments
                .Include(c => c.Place)
                .Where(c => c.AspNetUserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }
    }
}