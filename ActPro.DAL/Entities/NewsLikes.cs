namespace ActPro.DAL.Entities
{
    public class NewsLikes
    {
        public int Id { get; set; }
        public int NewsId { get; set; }
        public string UserId { get; set; }
    }
}