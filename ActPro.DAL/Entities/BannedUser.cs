namespace ActPro.DAL.Entities
{
    public class BannedUser
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime BannedAt { get; set; } = DateTime.Now;
        public string Reason { get; set; } = "Системна забрана";
    }
}
