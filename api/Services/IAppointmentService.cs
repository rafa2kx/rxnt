using RXNT.API.Models;
using RXNT.API.DTOs;

namespace RXNT.API.Services
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync();
        Task<AppointmentDto?> GetAppointmentByIdAsync(int id);
        Task<AppointmentDto> CreateAppointmentAsync(AppointmentDto appointment);
        Task<AppointmentDto?> UpdateAppointmentAsync(int id, AppointmentDto appointment);
        Task<bool> DeleteAppointmentAsync(int id);
        Task<(bool IsValid, string ErrorMessage)> ValidateAppointmentAsync(Appointment appointment);
        Task<int> BulkUpdateStatusAsync(string status, IEnumerable<int> appointmentIds);
    }
}
