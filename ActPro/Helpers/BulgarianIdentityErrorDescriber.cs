using Microsoft.AspNetCore.Identity;

namespace ActPro.Helpers
{
    public class BulgarianIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DuplicateEmail(string email)
            => new IdentityError { Code = nameof(DuplicateEmail), Description = MessageConstants.UserWithThisEmailExists };

        public override IdentityError DuplicateUserName(string userName)
            => new IdentityError { Code = nameof(DuplicateUserName), Description = MessageConstants.UserWithThisEmailExists };

        public override IdentityError PasswordRequiresDigit()
            => new IdentityError { Code = nameof(PasswordRequiresDigit), Description = MessageConstants.InvalidPasswordFormat };

        public override IdentityError PasswordRequiresLower()
            => new IdentityError { Code = nameof(PasswordRequiresLower), Description = MessageConstants.InvalidPasswordFormat };
        public override IdentityError PasswordRequiresUpper()
            => new IdentityError { Code = nameof(PasswordRequiresUpper), Description = MessageConstants.InvalidPasswordFormat };
        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
            => new IdentityError { Code = nameof(PasswordRequiresUniqueChars), Description = MessageConstants.InvalidPasswordFormat };
        public override IdentityError PasswordTooShort(int length)
            => new IdentityError { Code = nameof(PasswordTooShort), Description = $"Паролата трябва да е минимум {length} символа." };
        public override IdentityError DefaultError() 
            => new IdentityError { Code = nameof(DefaultError), Description = "Възникна неочаквана грешка." };
    }
}
