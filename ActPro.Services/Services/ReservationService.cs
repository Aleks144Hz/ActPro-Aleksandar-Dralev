using ActPro.DAL;
using ActPro.DAL.Entities;
using ActPro.Domain;
using ActPro.Domain.Models;
using ActPro.Domain.Models.Reservation;
using ActPro.Domain.Models.User;
using ActPro.Domain.Repository;
using ActPro.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ActPro.Services
{
    public class ReservationService(IRepository<Place> placeRepo, IRepository<Reservation> resRepo, IRepository<Comment> commentRepo, IRepository<PlaceClosure> closureRepo,
        IRepository<Favorite> favRepo, UserManager<ApplicationUser> userManager, IEmailSender emailSender) : IReservationService
    {
        // --INDEX
        public async Task<ReservationViewModel> GetReservationIndexModelAsync(int placeId, string userId)
        {
            var place = await placeRepo.AllAsNoTracking()
            .Include(p => p.Comments).ThenInclude(c => c.User)
            .Include(p => p.PlaceImages)
            .Include(p => p.City)
            .Include(p => p.Activity)
            .FirstOrDefaultAsync(m => m.Id == placeId);

            double userCredits = 0;
            if (userId != null)
            {
                var user = await userManager.FindByIdAsync(userId);
                userCredits = user?.Credits ?? 0;
            }

            if (place == null) return null;
            return new ReservationViewModel
            {
                Place = place,
                IsFavorite = userId != null && await favRepo.AllAsNoTracking()
                .AnyAsync(f => f.PlaceId == placeId && f.AspNetUserId == userId),
                CurrentUserId = userId,
                UserCredits = userCredits,
                UserCommentsCount = userId != null ? place.Comments.Count(c => c.AspNetUserId == userId) : 0
            };
        }

        // -- RESERVATION
        public async Task<(bool success, string message)> BookAsync(int placeId, DateTime date, string timeSlot, ApplicationUser user)
        {
            var closure = await closureRepo.AllAsNoTracking()
            .FirstOrDefaultAsync(c => c.PlaceId == placeId && c.ClosureDate.Date == date.Date);

            if (closure != null)
            {
                string reason = !string.IsNullOrEmpty(closure.Reason) ? closure.Reason : DomainResources.PlanedManufacture;
                return (false, $"{DomainResources.PlaceIsClosedOnThisDate} {reason}.");
            }

            if (!TimeOnly.TryParse(timeSlot, out TimeOnly parsedTime)) return (false, DomainResources.InvalidHour);

            DateOnly parsedDate = DateOnly.FromDateTime(date);
            var combinedDateTime = parsedDate.ToDateTime(parsedTime);

            if (combinedDateTime < DateTime.Now) return (false, DomainResources.HourAlreadyPassed);

            if (await resRepo.AllAsNoTracking().AnyAsync(r => r.PlaceId == placeId && r.ReservationDate == parsedDate && r.ReservationTime == parsedTime))
                return (false, DomainResources.HourAlreadyReserved);
            var place = await placeRepo.AllAsNoTracking()
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == placeId);

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

            await resRepo.AddAsync(reservation);
            user.Credits += 1;
            await userManager.UpdateAsync(user);
            await resRepo.SaveChangesAsync();
            if (place.Owner != null)
            {
                await emailSender.SendNewBookingNotificationAsync(
                    place.Owner.Email,
                    place.Owner.FirstName,
                    place.Name,
                    $"{user.FirstName} {user.LastName}",
                    parsedDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                    timeSlot,
                    user.PhoneNumber);
            }

            return (true, DomainResources.SuccessfullReservation);
        }
        //-- ADD REVIEW
        public async Task<(bool success, string message)> AddReviewAsync(int placeId, string userId, string commentText, int rating)
        {
            int userCommentCount = await commentRepo.AllAsNoTracking().CountAsync(c => c.PlaceId == placeId && c.AspNetUserId == userId);
            if (userCommentCount >= 3) return (false, DomainResources.MaximumComments);

            var newComment = new Comment
            {
                PlaceId = placeId,
                AspNetUserId = userId,
                CommentText = commentText,
                Rating = rating,
                CreatedAt = DateTime.Now
            };

            await commentRepo.AddAsync(newComment);
            await commentRepo.SaveChangesAsync();

            await UpdatePlaceRatingAsync(placeId);

            var user = await userManager.FindByIdAsync(userId);
            user.Credits = Math.Round(user.Credits + 0.1, 2);
            await userManager.UpdateAsync(user);
            var place = await placeRepo.AllAsNoTracking()
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == placeId);
            if (place?.Owner != null)
            {
                await emailSender.SendNewReviewNotificationAsync(
                    place.Owner.Email,
                    place.Owner.FirstName,
                    place.Name,
                    user.FirstName,
                    rating,
                    commentText);
            }

            return (true, DomainResources.CommentAdded);
        }

        //-- DELETE REVIEW
        public async Task<(bool success, string message)> DeleteReviewAsync(int reviewId, string userId)
        {
            var comment = await commentRepo.All().FirstOrDefaultAsync(c => c.Id == reviewId && c.AspNetUserId == userId);
            if (comment == null) return (false, DomainResources.CommentNotFound);

            int placeId = comment.PlaceId;
            await commentRepo.DeleteAsync(comment);
            await commentRepo.SaveChangesAsync();

            await UpdatePlaceRatingAsync(placeId);

            var user = await userManager.FindByIdAsync(userId);
            user.Credits = Math.Round(Math.Max(0, user.Credits - 0.1), 2);
            await userManager.UpdateAsync(user);

            return (true, DomainResources.CommentDeleted);
        }

        //-- UPDATE PLACE RATING
        private async Task UpdatePlaceRatingAsync(int placeId)
        {
            var place = await placeRepo.All().Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == placeId);
            if (place != null)
            {
                place.Rating = place.Comments.Any() ? (int)Math.Round(place.Comments.Average(c => (double)c.Rating)) : 0;
                await placeRepo.SaveChangesAsync();
            }
        }

        //-- GET OCCUPIED SLOTS
        public async Task<List<string>> GetOccupiedSlotsAsync(int placeId, DateTime date)
        {
            var parsedDate = DateOnly.FromDateTime(date);
            return await resRepo.AllAsNoTracking()
            .Where(r => r.PlaceId == placeId && r.ReservationDate == parsedDate)
            .Select(r => r.ReservationTime.Value.ToString("HH:mm"))
            .ToListAsync();
        }

        // -- GET USER RESERVATIONS
        public async Task<UserReservationsViewModel> GetUserReservationsAsync(string userId, int page, int pageSize, string filter)
        {
            var query = resRepo.AllAsNoTracking().Where(r => r.AspNetUserId == userId);
            var today = DateOnly.FromDateTime(DateTime.Now);
            var now = TimeOnly.FromDateTime(DateTime.Now);

            if (filter == "upcoming")
                query = query.Where(r => r.ReservationDate > today || (r.ReservationDate == today && r.ReservationTime > now));
            else if (filter == "past")
                query = query.Where(r => r.ReservationDate < today || (r.ReservationDate == today && r.ReservationTime <= now));

            int totalCount = await query.CountAsync();

            var reservationsData = await query
                .Include(r => r.Place).ThenInclude(p => p.PlaceImages)
                .Include(r => r.Place).ThenInclude(p => p.City)
                .OrderByDescending(r => r.ReservationDate).ThenByDescending(r => r.ReservationTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReservationItemViewModel
                {
                    Id = r.Id,
                    PlaceId = (int)r.PlaceId,
                    PlaceName = r.Place.Name,
                    CityName = r.Place.City.Name,
                    ReservationDate = r.ReservationDate,
                    ReservationTime = r.ReservationTime,
                    ImageUrl = r.Place.PlaceImages.OrderBy(i => i.Id).FirstOrDefault().ImageUrl
                })
                .ToListAsync();

            return new UserReservationsViewModel
            {
                Reservations = reservationsData,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                TotalCount = totalCount,
                SelectedFilter = filter
            };
        }

        //-- CANCEL RESERVATION
        public async Task<(bool success, string message)> CancelReservationAsync(int reservationId, string userId)
        {
            var res = await resRepo.All()
                .Include(r => r.Place)
                .ThenInclude(p => p.Owner)
                .FirstOrDefaultAsync(r => r.Id == reservationId && r.AspNetUserId == userId);

            if (res == null) return (false, DomainResources.ReservationNotFound);

            var resDateTime = (res.ReservationDate ?? DateOnly.FromDateTime(DateTime.Now)).ToDateTime(res.ReservationTime ?? new TimeOnly(0, 0));
            if (resDateTime < DateTime.Now) return (false, DomainResources.CannotCancelPastReservation);

            var owner = res.Place?.Owner;
            var placeName = res.Place?.Name ?? DomainResources.Place;
            var customerName = $"{res.FirstName} {res.LastName}";
            var dateFormatted = res.ReservationDate?.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) ?? "";
            var timeSlot = res.ReservationTime?.ToString() ?? "";
            var number = res.Phone ?? "";

            await resRepo.DeleteAsync(res);
            await resRepo.SaveChangesAsync();

            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.Credits = Math.Max(0, user.Credits - 1);
                await userManager.UpdateAsync(user);
            }

            if (owner != null)
            {
                try
                {
                    await emailSender.SendBookingCancellationToOwnerAsync(
                        owner.Email,
                        owner.FirstName,
                        placeName,
                        customerName,
                        dateFormatted,
                        timeSlot,
                        number
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{DomainResources.Error} {ex.Message}");
                }
            }

            return (true, DomainResources.Cancelled);
        }

        //-- GET USER REVIEWS
        public async Task<List<UserReviewItemViewModel>> GetUserReviewsAsync(string userId)
        {
            return await commentRepo.AllAsNoTracking()
                .Include(c => c.Place)
                .Where(c => c.AspNetUserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new UserReviewItemViewModel
                {
                    Id = c.Id,
                    PlaceId = c.PlaceId,
                    PlaceName = c.Place.Name,
                    CommentText = c.CommentText,
                    Rating = c.Rating,
                    CreatedAt = (DateTime)c.CreatedAt
                })
                .ToListAsync();
        }

        //-- EDIT REVIEW
        public async Task<(bool success, string message)> EditReviewAsync(int commentId, string userId, string commentText, int rating)
        {
            var comment = await commentRepo.All().FirstOrDefaultAsync(c => c.Id == commentId && c.AspNetUserId == userId);
            if (comment == null) return (false, DomainResources.NotFound);

            comment.CommentText = commentText;
            comment.Rating = rating;
            await commentRepo.SaveChangesAsync();
            await UpdatePlaceRatingAsync(comment.PlaceId);
            return (true, DomainResources.Update);
        }

        //-- GET CLOSED DATES
        public async Task<List<string>> GetClosedDatesAsync(int placeId)
        {
            var today = DateTime.Today;

            return await closureRepo.AllAsNoTracking()
                .Where(c => c.PlaceId == placeId && c.ClosureDate.Date >= today)
                .Select(c => c.ClosureDate.ToString("yyyy-MM-dd"))
                .ToListAsync();
        }
    }
}