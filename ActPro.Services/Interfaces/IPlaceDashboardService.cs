using ActPro.DAL.Entities;
using Microsoft.AspNetCore.Http;

namespace ActPro.Services.Interfaces
{
    public interface IPlaceDashboardService
    {
        Task<IEnumerable<Place>> GetAllPlacesAsync();
        Task<IEnumerable<Place>> GetOwnerPlacesAsync(string ownerId);
        Task<Place?> GetByIdAsync(int id);
        Task<bool> CreatePlaceAsync(Place place, IEnumerable<IFormFile>? images, string userId, bool isApproved);
        Task<bool> UpdatePlaceAsync(Place place, IEnumerable<IFormFile>? images, string? ownerId = null);
        Task<bool> DeletePlaceAsync(int id);
        Task<bool> ApprovePlaceAsync(int id);
        Task<bool> DeleteImageAsync(int imageId);
        Task<bool> AddClosuresAsync(int placeId, DateTime start, DateTime end, string reason);
        Task<bool> RemoveClosureAsync(int closureId);
    }
}