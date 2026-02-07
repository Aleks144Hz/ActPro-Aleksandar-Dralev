using ActPro.DAL;
using ActPro.DAL.Entities;
using ActPro.Domain.Models;
using ActPro.Domain.Repository;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IRepository<Place> _placeRepo;
        private readonly IRepository<Reservation> _resRepo;
        private readonly IRepository<Comment> _commentRepo;
        private readonly IRepository<PlaceClosure> _closureRepo;
        private readonly IRepository<Favorite> _favRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationService(IRepository<Place> placeRepo, IRepository<Reservation> resRepo, IRepository<Comment> commentRepo, IRepository<PlaceClosure> closureRepo, IRepository<Favorite> favRepo, UserManager<ApplicationUser> userManager)
        {
            _placeRepo = placeRepo;
            _resRepo = resRepo;
            _commentRepo = commentRepo;
            _closureRepo = closureRepo;
            _favRepo = favRepo;
            _userManager = userManager;
        }

        public async Task<ReservationViewModel> GetReservationIndexModelAsync(int placeId, string userId)
        {
            var place = await _placeRepo.AllAsNoTracking()
                .Include(p => p.Comments).ThenInclude(c => c.User)
                .Include(p => p.PlaceImages)
                .Include(p => p.City)
                .Include(p => p.Activity)
                .FirstOrDefaultAsync(m => m.Id == placeId);

            if (place == null) return null;

            return new ReservationViewModel
            {
                Place = place,
                IsFavorite = userId != null && await _favRepo.AllAsNoTracking()
                .AnyAsync(f => f.PlaceId == placeId && f.AspNetUserId == userId),
                CurrentUserId = userId,
                UserCommentsCount = userId != null ? place.Comments.Count(c => c.AspNetUserId == userId) : 0
            };
        }

        public async Task<(bool success, string message)> BookAsync(int placeId, DateTime date, string timeSlot, ApplicationUser user)
        {
            if (await _closureRepo.AllAsNoTracking().AnyAsync(c => c.PlaceId == placeId && c.ClosureDate.Date == date.Date))
                return (false, "Обектът е затворен за резервации на тази дата.");

            if (!TimeOnly.TryParse(timeSlot, out TimeOnly parsedTime)) return (false, "Невалиден час.");

            DateOnly parsedDate = DateOnly.FromDateTime(date);
            var combinedDateTime = parsedDate.ToDateTime(parsedTime);

            if (combinedDateTime < DateTime.Now) return (false, "Часът вече е минал.");

            if (await _resRepo.AllAsNoTracking().AnyAsync(r => r.PlaceId == placeId && r.ReservationDate == parsedDate && r.ReservationTime == parsedTime))
                return (false, "Този час току-що беше зает!");

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

            await _resRepo.AddAsync(reservation);
            user.Credits += 1;
            await _userManager.UpdateAsync(user);
            await _resRepo.SaveChangesAsync();

            return (true, "Резервацията е успешно направена.");
        }

        public async Task<(bool success, string message)> AddReviewAsync(int placeId, string userId, string commentText, int rating)
        {
            int userCommentCount = await _commentRepo.AllAsNoTracking().CountAsync(c => c.PlaceId == placeId && c.AspNetUserId == userId);
            if (userCommentCount >= 3) return (false, "Вече сте оставили максималния брой коментари (3) за този обект.");

            var newComment = new Comment
            {
                PlaceId = placeId,
                AspNetUserId = userId,
                CommentText = commentText,
                Rating = rating,
                CreatedAt = DateTime.Now
            };

            await _commentRepo.AddAsync(newComment);
            await _commentRepo.SaveChangesAsync();

            await UpdatePlaceRatingAsync(placeId);

            var user = await _userManager.FindByIdAsync(userId);
            user.Credits = Math.Round(user.Credits + 0.1, 2);
            await _userManager.UpdateAsync(user);

            return (true, "Коментарът е добавен успешно.");
        }

        public async Task<(bool success, string message)> DeleteReviewAsync(int reviewId, string userId)
        {
            var comment = await _commentRepo.All().FirstOrDefaultAsync(c => c.Id == reviewId && c.AspNetUserId == userId);
            if (comment == null) return (false, "Коментарът не е намерен.");

            int placeId = comment.PlaceId;
            await _commentRepo.DeleteAsync(comment);
            await _commentRepo.SaveChangesAsync();

            await UpdatePlaceRatingAsync(placeId);

            var user = await _userManager.FindByIdAsync(userId);
            user.Credits = Math.Round(Math.Max(0, user.Credits - 0.1), 2);
            await _userManager.UpdateAsync(user);

            return (true, "Коментарът е изтрит успешно.");
        }

        private async Task UpdatePlaceRatingAsync(int placeId)
        {
            var place = await _placeRepo.All().Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == placeId);
            if (place != null)
            {
                place.Rating = place.Comments.Any() ? (int)Math.Round(place.Comments.Average(c => (double)c.Rating)) : 0;
                await _placeRepo.SaveChangesAsync();
            }
        }

        public async Task<List<string>> GetOccupiedSlotsAsync(int placeId, DateTime date)
        {
            var parsedDate = DateOnly.FromDateTime(date);
            return await _resRepo.AllAsNoTracking()
            .Where(r => r.PlaceId == placeId && r.ReservationDate == parsedDate)
            .Select(r => r.ReservationTime.Value.ToString("HH:mm"))
            .ToListAsync();
        }

        public async Task<(IEnumerable<Reservation> reservations, int totalCount)> GetUserReservationsAsync(string userId, int page, int pageSize, string filter)
        {
            var query = _resRepo.AllAsNoTracking().Where(r => r.AspNetUserId == userId);
            var today = DateOnly.FromDateTime(DateTime.Now);
            var now = TimeOnly.FromDateTime(DateTime.Now);

            if (filter == "upcoming")
                query = query.Where(r => r.ReservationDate > today || (r.ReservationDate == today && r.ReservationTime > now));
            else if (filter == "past")
                query = query.Where(r => r.ReservationDate < today || (r.ReservationDate == today && r.ReservationTime <= now));

            int total = await query.CountAsync();
            var list = await query.Include(r => r.Place).ThenInclude(p => p.PlaceImages)
            .Include(r => r.Place).ThenInclude(p => p.City)
            .OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.ReservationTime)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (list, total);
        }

        public async Task<(bool success, string message)> CancelReservationAsync(int reservationId, string userId)
        {
            var res = await _resRepo.All().FirstOrDefaultAsync(r => r.Id == reservationId && r.AspNetUserId == userId);
            if (res == null) return (false, "Резервацията не е намерена.");

            var resDateTime = (res.ReservationDate ?? DateOnly.FromDateTime(DateTime.Now)).ToDateTime(res.ReservationTime ?? new TimeOnly(0, 0));
            if (resDateTime < DateTime.Now) return (false, "Не можете да откажете изминала резервация.");

            await _resRepo.DeleteAsync(res);
            await _resRepo.SaveChangesAsync();
            var user = await _userManager.FindByIdAsync(userId);
            user.Credits = Math.Max(0, user.Credits - 1);
            await _userManager.UpdateAsync(user);
            return (true, "Успешно отказана.");
        }

        public async Task<List<Comment>> GetUserReviewsAsync(string userId) => await _commentRepo.AllAsNoTracking()
        .Include(c => c.Place)
        .Where(c => c.AspNetUserId == userId)
        .OrderByDescending(c => c.CreatedAt)
        .ToListAsync();

        public async Task<(bool success, string message)> EditReviewAsync(int commentId, string userId, string commentText, int rating)
        {
            var comment = await _commentRepo.All().FirstOrDefaultAsync(c => c.Id == commentId && c.AspNetUserId == userId);
            if (comment == null) return (false, "Не е намерен.");

            comment.CommentText = commentText;
            comment.Rating = rating;
            await _commentRepo.SaveChangesAsync();
            await UpdatePlaceRatingAsync(comment.PlaceId);
            return (true, "Обновено.");
        }
    }
}