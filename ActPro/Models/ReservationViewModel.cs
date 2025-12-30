using ActPro.DAL.Entities;

namespace ActPro.Models
{
    internal class ReservationViewModel
    {
        public Place Place { get; set; }
        public bool IsFavorite { get; set; }
    }
}