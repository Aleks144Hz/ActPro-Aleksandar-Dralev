namespace ActPro.Models.User
{
    public class ProfileSettingsViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfilePicturePath { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}