using ActPro.DAL.Entities;

namespace ActPro.Domain.Models.Areas
{
    public class OwnerDashboardViewModel
    {
        public OwnerDashboardViewModel()
        {
            MyPlaces = new List<Place>();
            RecentReservations = new List<DAL.Entities.Reservation>();
        }

        public List<Place> MyPlaces { get; set; }
        public List<DAL.Entities.Reservation> RecentReservations { get; set; }
        public decimal TotalIncome { get; set; }
        public int TotalReservationsCount { get; set; }
    }
}
