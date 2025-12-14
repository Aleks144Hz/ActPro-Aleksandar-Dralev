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
        public const string InvalidIdCardNumber = "Моля, въведете валиден номер на лична карта.";
        public const string InvalidIdUinNumber = "Моля, въведете валиден егн номер.";
        public const string InvalidDDSNumber = "Моля, въведете валиден ДДС номер.";
        public const string InvalidIban = "Моля, въведете валиден номер на банкова сметка.";
        public const string InvalidEik = "Моля, въведете валиден ЕИК номер.";
        public const string InvalidVatNumber = "Моля, въведете валиден ИН по ДДС.";
        public const string InvalidPostCode = "Моля, въведете валиден пощенски код.";
        public const string InvalidIkoCode = "Моля, въведете валиден ИКО код.";
        public const string InvalidPikCodeNap = "Моля, въведете валиден ПИК код на НАП.";
        public const string InvalidPikCodeNoi = "Моля, въведете валиден ПИК код на НОИ.";
        public const string InvalidInvoiceNumber = "Моля, въведете валиден номер на фактура.";
        public const string InvalidInitialCashBalance = "Моля, въведете валидна касова наличност.";
        public const string InvalidOTPCode = "Невалиден ОТP код!";
        public const string AddressNotValid = "Полето може да съдържа букви, цифри и символи ““,.- №.";
        public const string Error = "Възникна грешка.";
        public const string FieldRequired = "Това поле е задължително!";
        public const string OnlyLettersAllowed = "Полето може да съдържа само букви.";
        public const string OnlyNumbersAllowed = "Полето може да съдържа само цифри.";
        public const string OnlyDecimalNumbersAllowed = "Полето може да съдържа само десетични числа.";
        public const string LettersNumbersAllowed = "Полето може да съдържа букви и цифри";
        public const string LettersNumbersEnglAllowed = "Полето може да съдържа латински букви и цифри";
        public const string RoleNotFound = "Не съществува такава роля.";
        public const string ContractorNotFound = "Не е намерен контрагент.";
        public const string CompanyExistsEik = "Вече съществува компания с това ЕИК.";
        public const string NotExistingCompany = "Няма съществуваща компания.";
        public const string MinimumLengthThreeSymbolsErrorMessage = "Полето може да съдържа минимум 3 символа.";
        public const string FieldNotValid = "Невалиден формат.";
        public const string ErrorWhileCreatingCheckData = "Грешка при създаване, проверете въведените данни!";
        public const string ServiceInUseCannotBeDeleted = "Услугата в момента е активна и не може да бъде изтрита!";
        public const string UserIsNotRegistered = "Потребителят не съществува в системата! Моля регистрирайте го!";
        public const string NotValidPassword = "Невалидна парола! Моля проверете отново!";
        public const string EIKFieldRequired = "Еик е задължително!";
        public const string ContractorNameFieldRequired = "Име на контрагент е задължително!";
        public const string DocumentTypeFieldRequired = "Тип документ е задължително!";
        public const string VATOperationFieldRequired = "ДДС операция е задължително!";
        public const string DocumentNumFieldRequired = "Номер на документ е задължително!";
        public const string InvoiceDateFutureDateInvalid = "Невалидна дата на документ!";
        public const string AccountingMonthFutureDateInvalid = "Невалиден счетоводен месец!";
        public const string VATMonthFutureDateInvalid = "Невалиден ДДС месец!";
        public const string InvalidAccountingDocument = "Неуспешно осчетоводяване на документ";
        public const string NoMainContoursError = "Няма въведени основни контировки";
        public const string NoCashEntitiesEnteredError = "Няма въведени данни за касови плащания";
        public const string ErrorLoadingContractorData = "Грешка при зареждане на детайлите на партньора.";
        public const string ErrorSavingContractorData = "Грешка при запазване на данните за партньора.";
        public const string ErrorDeletingContractorData = "Грешка при изтриване на данните за партньора.";
        public const string InvalidPasswordFormat =
            "Невалидна парола. Паролата трябва да съдържа поне 8 символа,\nот които поне една цифра, една главна и една малка буква,\nкакто и поне един специален символ";

        public const string UserWithThisEmailExists = "Потребител с този имейл вече съществува.";
        public const string InvalidInputData = "Въведените данни са невалидни.";
        public const string ContractWithCompanyMissingError = "Договор със счетоводната фирма е задължителен и липсва.";
        public const string NotExistingAccount = "Не съществува такава сметка.";
        public const string InvalidParentTypeId = "Не съществува родителска сметка.";
        public const string InvalidSyntheticAccountData = "Невалидни данни за синтетична сметка.";
        public const string ErrorCreateSyntheticAccount = "Възникна грешка при създаването на синтетична сметка.";
        public const string ErrorCreateAnalyticAccount = "Възникна грешка при създаването на аналитична сметка.";
        public const string ErrorDeleteAccount = "Грешка при изтриване на сметка.";
        public const string ErrorEditAccount = "Грешка при редактиране на сметка.";
        public const string NotExistingAccountsForCompany = "Няма съществуващи сметки в сметкоплана на компания.";
        public const string ErrorDeletingAccountsForCompany = "Грешка при изтриване на сметкоплан за компания.";
        public const string MissingFileError = "Не съществува такъв файл";
        public const string DuplicateSubcategoryNameError = "Подкатегория с такова име съществува";

        //Success
        public const string SuccessfullOperation = "Успешно извършена операция!";
        public const string SuccessfulContractorAdded = "Успешно добавен контрагент.";
        public const string SuccessfulServiceAdded = "Успешно създадена услуга!";
        public const string SuccessfulUpdatedService = "Успешно редактирана услуга!";
        public const string SuccessfulDeletedService = "Успешно изтрита услуга!";
        public const string SuccessfulCompanyServicesUpdated = "Успешно обновени услуги!";
        public const string SuccessfulUpdatedServicesForApproval = "Успешно редактирани услуги за одобрение!";
        public const string SuccessfulAccountingDocument = "Успешно осчетоводен документ";
        public const string SuccessfulAccountingRecordAdded = "Успешно създаден счетоводен запис";
        public const string SuccessfulAccountingRecordUpdated = "Успешно редактиран счетоводен запис";
        public const string SuccessfulUploadedFiles = "Успешно прикачени файлове";
        public const string SuccessfulInvoiceDelete = "Успешно изтрит запис.";
        public const string SuccessfulInvoiceMove = "Успешно преместен запис";
        public const string SuccessfulInvoiceChangeType = "Успешно сменен типът на записа";
        public const string SuccessfulDocumentUnite = "Успешно обединихте документите";
        public const string SuccessSaveSyntheticAccount = "Синтетична сметка беше успешно създадена.";
        public const string SuccessDeleteAccount = "Успешно изтрихте сметка.";
        public const string SuccessEditAccount = "Успешно редактирахте сметка.";
        public const string SuccessDeletingAccountsForCompany = "Успешно изтриване на сметкоплан за компания.";
        public const string SuccessfulInvoiceProcess = "Успешно обработихте документа";
        public const string SuccessfulUserEdit = "Успешно редактирахте данните на потребителя";
        public const string SuccessfulSubcategoryCreate = "Успешно запазихте подкатегориите";
        public const string SuccessfulSubcategoryUpdate = "Успешно редактирахте подкатегория";
        public const string SuccessfulSubcategoryDelete = "Успешно изтрихте подкатегория";
        public const string SuccessfulSaveChanges = "Успешно запазихте промените";
    }
}
