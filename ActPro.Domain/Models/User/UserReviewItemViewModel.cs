namespace ActPro.Domain.Models.Reservation
{
    public class UserReviewItemViewModel
    {
        public int Id { get; set; }
        public int PlaceId { get; set; }
        public string PlaceName { get; set; } = string.Empty;
        public string? PlaceNameEn { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}