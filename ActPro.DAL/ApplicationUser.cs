using ActPro.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace ActPro.DAL
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? ProfilePicturePath { get; set; }
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public double Credits { get; set; } = 0;
    }
}
