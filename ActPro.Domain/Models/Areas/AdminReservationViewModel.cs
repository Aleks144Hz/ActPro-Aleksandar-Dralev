namespace ActPro.Domain.Models.Areas
{
    public class ReservationsIndexViewModel
    {
        public IEnumerable<ReservationItemViewModel> Reservations { get; set; } = new List<ReservationItemViewModel>();
    }

    public class ReservationItemViewModel
    {
        public int Id { get; set; }
        public int PlaceId { get; set; }
        public string? PlaceName { get; set; }
        public string? PlaceNameEn { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string? Phone { get; set; }
        public DateOnly? ReservationDate { get; set; }
        public TimeOnly? ReservationTime { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class EditTimeViewModel
    {
        public int Id { get; set; }
        public string? PlaceName { get; set; }
        public string? PlaceNameEn { get; set; }
        public string? CustomerName { get; set; }
        public string? ReservationTime { get; set; }
    }
}