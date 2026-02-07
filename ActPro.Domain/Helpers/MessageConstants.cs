namespace ActPro.Helpers
{
    public class MessageConstants
    {
        //Error
        public const string NameIsRequired = "Името е задължително!";
        public const string LastNameIsRequired = "Фамилията е задължителнa!";
        public const string Email = "Моля, въведете валиден имейл адрес.";
        public const string EmailIsRequired = "Имейлa е задължителен!";
        public const string PasswordMismatch = "Паролите трябва да съвпадат.";
        public const string PasswordIsRequired = "Паролата е задължителна!";
        public const string ConfirmPasswordIsRequired = "Паролата за потвърждение е задължителна!";
        public const string InvalidPhoneNumber = "Моля, въведете валиден телефонен номер.";
        public const string PhoneNumberRequired = "Телефонен номер е задължителен!";
        public const string AddressNotValid = "Полето може да съдържа букви, цифри и символи ““,.- №.";
        public const string Error = "Възникна грешка.";
        public const string OnlyLettersAllowed = "Полето може да съдържа само букви.";
        public const string OnlyNumbersAllowed = "Полето може да съдържа само цифри.";
        public const string OnlyDecimalNumbersAllowed = "Полето може да съдържа само десетични числа.";
        public const string LettersNumbersAllowed = "Полето може да съдържа букви и цифри";
        public const string LettersNumbersEnglAllowed = "Полето може да съдържа латински букви и цифри";
        public const string RoleNotFound = "Не съществува такава роля.";
        public const string ErrorWhileCreatingCheckData = "Грешка при създаване, проверете въведените данни!";
        public const string UserIsNotRegistered = "Потребителят не съществува в системата!";
        public const string NotValidPassword = "Невалидна парола! Моля проверете отново!";
        public const string InvalidPasswordFormat =
            "Невалидна парола. Паролата трябва да е изписана на латиница, да съдържа поне 6 символа,\nот които поне една цифра, една главна и една малка буква,\nкакто и поне един специален символ.";
        public const string UserWithThisEmailExists = "Потребител с този имейл вече съществува.";

        //Success
        public const string SuccessfulDeletedAccount = "Профилът бе изтрит! До нови срещи.";
        public const string SuccessfulUserEdit = "Успешно редактирахте данните на потребителя";
        public const string SuccessfulSaveChanges = "Успешно запазихте промените";
    }
}
