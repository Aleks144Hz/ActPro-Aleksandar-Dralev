using System.ComponentModel.DataAnnotations.Schema;

namespace ActPro.DAL.Entities
{
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? TitleEn { get; set; }
        public string Content { get; set; }
        public string? ContentEn { get; set; }
        public string? ImageURL { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int Likes { get; set; } = 0;
        [NotMapped]
        public bool IsLikedByCurrentUser { get; set; }
    }
}