using RXNT.API.Models;

namespace RXNT.API.Services
{
    public interface IPatientValidationService
    {
        Task<(bool IsValid, string ErrorMessage)> ValidatePatientAsync(Patient patient, bool isUpdate = false, int? existingId = null);
        bool ValidateDateOfBirth(DateTime dateOfBirth);
        bool ValidateEmail(string email);
        bool ValidatePhone(string phone);
        bool ValidateName(string name);
    }
}
