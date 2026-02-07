using ActPro.DAL.Entities;

namespace ActPro.Models.User
{
    public class UserProfileViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfilePicturePath { get; set; }
        public double Credits { get; set; }
        public int ReservationsCount { get; set; }
        public int ReviewsCount { get; set; }
        public IEnumerable<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}