using ActPro.DAL.Entities;

namespace ActPro.Domain.Models
{
    public class ReservationViewModel
    {
        public Place Place { get; set; }

        public DateTime SelectedDate { get; set; } = DateTime.Now;
        public string SelectedTimeSlot { get; set; }
        public bool IsFavorite { get; set; }
        public int UserCommentsCount { get; set; }
        public bool CanUserLeaveComment => UserCommentsCount < 3;
        public string CurrentUserId { get; set; }
    }
}