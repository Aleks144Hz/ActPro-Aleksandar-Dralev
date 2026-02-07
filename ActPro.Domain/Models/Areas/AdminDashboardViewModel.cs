namespace ActPro.Domain.Models.Areas
{
    public class AdminDashboardViewModel
    {
        public int TotalReservations { get; set; }
        public int TotalUsers { get; set; }
        public int TotalPlaces { get; set; }
        public int PendingComments { get; set; }
        public IEnumerable<ActPro.DAL.Entities.Reservation> LatestReservations { get; set; }
    }
}
