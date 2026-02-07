using ActPro.DAL.Entities;

namespace ActPro.Domain.Models.Areas
{
    public class OwnerDashboardViewModel
    {
        public OwnerDashboardViewModel()
        {
            MyPlaces = new List<Place>();
            RecentReservations = new List<Reservation>();
        }
        public List<Place> MyPlaces { get; set; }
        public List<Reservation> RecentReservations { get; set; }
        public decimal TotalIncome { get; set; }
        public int TotalReservationsCount { get; set; }
    }
}
