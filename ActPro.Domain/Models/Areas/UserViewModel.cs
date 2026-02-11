namespace ActPro.Domain.Models.Areas
{
    public class UsersIndexViewModel
    {
        public IEnumerable<UserItemViewModel> Users { get; set; } = new List<UserItemViewModel>();
    }

    public class UserItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public bool IsAdmin { get; set; }
        public bool IsOwner { get; set; }
    }
}