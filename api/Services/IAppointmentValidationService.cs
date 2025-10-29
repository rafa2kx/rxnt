using RXNT.API.Models;

namespace RXNT.API.Services
{
    public interface IAppointmentValidationService
    {
        Task<(bool IsValid, string ErrorMessage)> ValidateAppointmentAsync(Appointment appointment);
        bool ValidateAppointmentDate(DateTime appointmentDate, string appointmentTime);
        bool ValidateStatus(string status);
    }
}
