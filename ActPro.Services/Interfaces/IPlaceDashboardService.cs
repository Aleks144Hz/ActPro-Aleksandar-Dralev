using ActPro.DAL.Entities;
using ActPro.Domain.Models.Areas;
using Microsoft.AspNetCore.Http;

namespace ActPro.Services.Interfaces
{
    public interface IPlaceDashboardService
    {
        Task<PlacesIndexViewModel> GetPlacesDashboardModelAsync(string? ownerId = null);
        Task<Place?> GetByIdAsync(int value);
        Task<bool> CreatePlaceAsync(Place place, IEnumerable<IFormFile>? images, string userId, bool isApproved);
        Task<bool> UpdatePlaceAsync(Place place, IEnumerable<IFormFile>? images, string? ownerId = null);
        Task<bool> DeletePlaceAsync(int id);
        Task<bool> ApprovePlaceAsync(int id);
        Task<bool> DeleteImageAsync(int imageId);
        Task<bool> AddClosuresAsync(int placeId, DateTime start, DateTime end, string reason);
        Task<bool> RemoveClosureAsync(int closureId);
    }
}