using ActPro.DAL.Data;
using ActPro.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ActPro.Services.Services
{
    public class IReservationDashboardService : Interfaces.IReservationDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public IReservationDashboardService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IEnumerable<Reservation>> GetAllReservationsAsync()
        {
            return await _context.Reservations
                .Include(r => r.Place)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetOwnerReservationsAsync(string ownerId)
        {
            return await _context.Reservations
                .Include(r => r.Place)
                .Where(r => r.Place.OwnerId == ownerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Reservation?> GetByIdAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.Place)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> DeleteReservationAsync(int id, string? ownerId = null)
        {
            var res = await GetByIdAsync(id);
            if (res == null) return false;

            if (ownerId != null && res.Place.OwnerId != ownerId) return false;

            _context.Reservations.Remove(res);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Delete Reservation", "Reservation", id.ToString(), $"Изтрита резервация на {res.FirstName} {res.LastName} за {res.ReservationDate:dd.MM.yyyy}");

            return true;
        }

        public async Task<bool> UpdateReservationTimeAsync(int id, TimeOnly newTime, string? ownerId = null)
        {
            var res = await GetByIdAsync(id);
            if (res == null) return false;
            if (ownerId != null && res.Place.OwnerId != ownerId) return false;

            var oldTime = res.ReservationTime;
            res.ReservationTime = newTime;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("Edit Reservation", "Reservation", id.ToString(), $"Променен час за {res.FirstName}: {oldTime} -> {newTime}");

            return true;
        }
    }
}