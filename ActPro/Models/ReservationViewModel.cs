using ActPro.DAL.Entities;

namespace ActPro.Models
{
    public class ReservationViewModel
    {
        public Place Place { get; set; }
        public bool IsFavorite { get; set; }
    }
}