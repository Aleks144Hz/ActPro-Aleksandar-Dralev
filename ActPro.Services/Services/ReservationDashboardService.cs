using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services.Services
{
    public class IReservationDashboardService(ApplicationDbContext context, IAuditService auditService) : Interfaces.IReservationDashboardService
    {
        public async Task<IEnumerable<Reservation>> GetAllReservationsAsync()
        {
            return await context.Reservations
            .Include(r => r.Place)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetOwnerReservationsAsync(string ownerId)
        {
            return await context.Reservations
            .Include(r => r.Place)
            .Where(r => r.Place.OwnerId == ownerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
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

            await auditService.LogAsync("Edit Reservation", "Reservation", id.ToString(), $"Променен час за {res.FirstName}: {oldTime} -> {newTime}");

            return true;
        }
    }
}