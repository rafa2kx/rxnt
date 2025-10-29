using RXNT.API.Models;

namespace RXNT.API.Services
{
    public interface IDoctorValidationService
    {
        Task<(bool IsValid, string ErrorMessage)> ValidateDoctorAsync(Doctor doctor, bool isUpdate = false, int? existingId = null);
    }
}
