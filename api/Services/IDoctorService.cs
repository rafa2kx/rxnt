using RXNT.API.Models;
using RXNT.API.DTOs;

namespace RXNT.API.Services
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync();
        Task<DoctorDto?> GetDoctorByIdAsync(int id);
        Task<DoctorDto> CreateDoctorAsync(DoctorDto doctor);
        Task<DoctorDto?> UpdateDoctorAsync(int id, DoctorDto doctor);
        Task<bool> DeleteDoctorAsync(int id);
        Task<IEnumerable<AppointmentDto>> GetDoctorScheduleAsync(int doctorId);
        Task<(bool IsValid, string ErrorMessage)> ValidateDoctorAsync(Doctor doctor, bool isUpdate = false, int? existingId = null);
    }
}
