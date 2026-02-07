using ActPro.DAL.Entities;
using ActPro.Models;
using Microsoft.AspNetCore.Http;

namespace ActPro.Services.Interfaces
{
    public interface IHomeService
    {
        Task<HomeViewModel> GetHomeViewModelAsync();
        Task<(IEnumerable<News> news, int totalPages)> GetNewsPagedAsync(int page, int pageSize);
        Task CreateNewsAsync(News news, IFormFile? imageFile, string webRootPath);
        Task<bool> DeleteNewsAsync(int id);
    }
}