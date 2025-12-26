namespace ActPro.Models
{
    public class MessageConstants
    {
        //Error
        public const string FieldNameRequired = "Моля, попълнете полето";
        public const string NameIsRequired = "Моля попълнете полето за Име";
        public const string Email = "Моля, въведете валиден имейл адрес.";
        public const string EmailIsRequired = "Полето Email е задължително";
        public const string CyrilicCharactersOnly = "Полето може да е попълнено само на кирилица.";
        public const string PasswordMismatch = "Паролите трябва да съвпадат.";
        public const string PasswordIsRequired = "Полето за парола е задължително!";
        public const string InvalidPhoneNumber = "Моля, въведете валиден телефонен номер.";
        public const string AddressNotValid = "Полето може да съдържа букви, цифри и символи ““,.- №.";
        public const string Error = "Възникна грешка.";
        public const string FieldRequired = "Това поле е задължително!";
        public const string OnlyLettersAllowed = "Полето може да съдържа само букви.";
        public const string OnlyNumbersAllowed = "Полето може да съдържа само цифри.";
        public const string OnlyDecimalNumbersAllowed = "Полето може да съдържа само десетични числа.";
        public const string LettersNumbersAllowed = "Полето може да съдържа букви и цифри";
        public const string LettersNumbersEnglAllowed = "Полето може да съдържа латински букви и цифри";
        public const string RoleNotFound = "Не съществува такава роля.";
        public const string MinimumLengthThreeSymbolsErrorMessage = "Полето може да съдържа минимум 3 символа.";
        public const string FieldNotValid = "Невалиден формат.";
        public const string ErrorWhileCreatingCheckData = "Грешка при създаване, проверете въведените данни!";
        public const string ServiceInUseCannotBeDeleted = "Услугата в момента е активна и не може да бъде изтрита!";
        public const string UserIsNotRegistered = "Потребителят не съществува в системата! Моля регистрирайте го!";
        public const string NotValidPassword = "Невалидна парола! Моля проверете отново!";
        public const string InvalidPasswordFormat =
            "Невалидна парола. Паролата трябва да съдържа поне 8 символа,\nот които поне една цифра, една главна и една малка буква,\nкакто и поне един специален символ";
        public const string UserWithThisEmailExists = "Потребител с този имейл вече съществува.";
        public const string InvalidInputData = "Въведените данни са невалидни.";
        public const string MissingFileError = "Не съществува такъв файл";

        //Success
        public const string SuccessfullOperation = "Успешно извършена операция!";
        public const string SuccessfulServiceAdded = "Успешно създадена услуга!";
        public const string SuccessfulUpdatedService = "Успешно редактирана услуга!";
        public const string SuccessfulDeletedService = "Успешно изтрита услуга!";
        public const string SuccessfulCompanyServicesUpdated = "Успешно обновени услуги!";
        public const string SuccessfulUploadedFiles = "Успешно прикачени файлове";
        public const string SuccessfulInvoiceDelete = "Успешно изтрит запис.";
        public const string SuccessfulInvoiceMove = "Успешно преместен запис";
        public const string SuccessfulInvoiceChangeType = "Успешно сменен типът на записа";
        public const string SuccessfulUserEdit = "Успешно редактирахте данните на потребителя";
        public const string SuccessfulSubcategoryCreate = "Успешно запазихте подкатегориите";
        public const string SuccessfulSubcategoryUpdate = "Успешно редактирахте подкатегория";
        public const string SuccessfulSubcategoryDelete = "Успешно изтрихте подкатегория";
        public const string SuccessfulSaveChanges = "Успешно запазихте промените";
    }
}
