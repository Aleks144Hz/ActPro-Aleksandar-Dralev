using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using ActPro.Domain.Models.Areas;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services
{
    public class IReservationDashboardService(ApplicationDbContext context, IAuditService auditService) : Interfaces.IReservationDashboardService
    {
        //-- Admin Dashboard: Reservations Management --
        public async Task<ReservationsIndexViewModel> GetReservationsIndexModelAsync(string? ownerId = null)
        {
            var query = context.Reservations
            .Include(r => r.Place)
            .AsQueryable();

            if (!string.IsNullOrEmpty(ownerId))
            {
                query = query.Where(r => r.Place.OwnerId == ownerId);
            }

            var reservations = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

            return new ReservationsIndexViewModel
            {
                Reservations = reservations.Select(r => new ReservationItemViewModel
                {
                    Id = r.Id,
                    PlaceId = r.PlaceId ?? 0,
                    PlaceName = r.Place?.Name ?? "Няма обект",
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    Phone = r.Phone,
                    ReservationDate = r.ReservationDate,
                    ReservationTime = r.ReservationTime,
                    CreatedAt = r.CreatedAt ?? DateTime.Now
                }).ToList()
            };
        }

        public async Task<Reservation?> GetByIdAsync(int id)
        {
            return await context.Reservations
            .Include(r => r.Place)
            .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> DeleteReservationAsync(int id, string? ownerId = null)
        {
            var res = await GetByIdAsync(id);
            if (res == null) return false;

            if (ownerId != null && res.Place.OwnerId != ownerId) return false;

            context.Reservations.Remove(res);
            await context.SaveChangesAsync();

            await auditService.LogAsync("Delete Reservation", "Reservation", id.ToString(), $"Изтрита резервация на {res.FirstName} {res.LastName} за {res.ReservationDate:dd.MM.yyyy}");

            return true;
        }

        public async Task<bool> UpdateReservationTimeAsync(int id, TimeOnly newTime, string? ownerId = null)
        {
            var res = await GetByIdAsync(id);
            if (res == null) return false;
            if (ownerId != null && res.Place.OwnerId != ownerId) return false;

            var oldTime = res.ReservationTime;
            res.ReservationTime = newTime;
            await context.SaveChangesAsync();

            await auditService.LogAsync("Edit Reservation", "Reservation", id.ToString(), $"Променен час за {res.FirstName} {res.LastName}: {oldTime} -> {newTime}");

            return true;
        }

        public Task<(bool success, string message)> LockSlotAsync(int placeId, DateTime date, string timeSlot)
        {
            throw new NotImplementedException();
        }
    }
}