using Microsoft.AspNetCore.Http;

namespace ActPro.Domain.Models
{
    public class NewsViewModel
    {
        public IEnumerable<DAL.Entities.News> NewsItems { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public string? NewTitle { get; set; }
        public string? NewContent { get; set; }
        public IFormFile? ImageFile { get; set; }
        public int Likes { get; set; }
    }
}