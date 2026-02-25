using Microsoft.AspNetCore.Identity;
using ActPro.Domain;

namespace ActPro.Helpers
{
    public class BulgarianIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DuplicateEmail(string email)
            => new IdentityError { Code = nameof(DuplicateEmail), Description = DomainResources.UserWithThisEmailExists };

        public override IdentityError DuplicateUserName(string userName)
            => new IdentityError { Code = nameof(DuplicateUserName), Description = DomainResources.UserWithThisEmailExists };

        public override IdentityError PasswordRequiresDigit()
            => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = DomainResources.InvalidPasswordFormat };

        public override IdentityError PasswordRequiresLower()
            => new IdentityError { Code = nameof(PasswordRequiresLower), Description = DomainResources.InvalidPasswordFormat };
        public override IdentityError PasswordRequiresUpper()
            => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = DomainResources.InvalidPasswordFormat };
        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
            => new IdentityError { Code = nameof(PasswordRequiresUniqueChars), Description = DomainResources.InvalidPasswordFormat };
        public override IdentityError PasswordTooShort(int length)
            => new IdentityError { Code = nameof(PasswordTooShort), Description = string.Format(DomainResources.PasswordTooShort, length) };
        public override IdentityError DefaultError() 
            => new IdentityError { Code = nameof(DefaultError), Description = DomainResources.DefaultError };
    }
}
