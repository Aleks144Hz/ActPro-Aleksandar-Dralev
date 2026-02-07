using Microsoft.AspNetCore.Identity;

namespace ActPro.Helpers
{
    public static class IdentityHelper
    {
        public static void GetIdentityOptions(IdentityOptions options)
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = true;
        }
    }
}
