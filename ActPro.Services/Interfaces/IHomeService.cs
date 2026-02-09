using ActPro.DAL.Entities;
using ActPro.Models;
using Microsoft.AspNetCore.Http;

namespace ActPro.Services.Interfaces
{
    public interface IHomeService
    {             
        Task<bool> DeleteNewsAsync(int id, string webRootPath);
        Task<HomeViewModel> GetHomeViewModelAsync();
        Task CreateNewsAsync(News news, IFormFile? imageFile, string webRootPath);
        Task<(IEnumerable<News> news, int totalPages)> GetNewsPagedAsync(int page, int pageSize, string? userId);
        Task<(int likes, bool isLiked)> LikeNewsAsync(int newsId, string userId);
    }
}