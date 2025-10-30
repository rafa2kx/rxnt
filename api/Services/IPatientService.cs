using RXNT.API.Models;
using RXNT.API.DTOs;

namespace RXNT.API.Services
{
    public interface IPatientService
    {
        Task<IEnumerable<PatientDto>> GetAllPatientsAsync();
        Task<PatientDto?> GetPatientByIdAsync(int id);
        Task<PatientDto> CreatePatientAsync(PatientDto patient, bool save = false);
        Task<PatientDto?> UpdatePatientAsync(int id, PatientDto patient);
        Task<bool> DeletePatientAsync(int id);
        Task<(bool IsValid, string ErrorMessage)> ValidatePatientAsync(Patient patient, bool isUpdate = false, int? existingId = null);
    }
}
