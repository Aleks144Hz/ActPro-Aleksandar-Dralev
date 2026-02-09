using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActPro.Domain.Models.User
{
    public class UserReservationsViewModel
    {
        public IEnumerable<ReservationItemViewModel> Reservations { get; set; } = new List<ReservationItemViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public string SelectedFilter { get; set; } = "all";
    }

    public class ReservationItemViewModel
    {
        public int Id { get; set; }
        public int PlaceId { get; set; }
        public string PlaceName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? CityName { get; set; }
        public DateOnly? ReservationDate { get; set; }
        public TimeOnly? ReservationTime { get; set; }

        public bool IsPast => ReservationDate.HasValue && ReservationTime.HasValue? ReservationDate.Value.ToDateTime(ReservationTime.Value) < DateTime.Now : false;
    }
}
