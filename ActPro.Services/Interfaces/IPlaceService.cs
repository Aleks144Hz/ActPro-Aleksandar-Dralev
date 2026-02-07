using ActPro.DAL.Entities;
using Microsoft.AspNetCore.Http;

namespace ActPro.Services.Interfaces
{
    public interface IPlaceService
    {
        Task<IEnumerable<City>> GetCitiesAsync();
        Task<IEnumerable<Activity>> GetActivitiesAsync();
        Task<bool> CreatePlaceRequestAsync(Place place, IEnumerable<IFormFile>? imageFiles, string userId, string webRootPath);
    }
}
