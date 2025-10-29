using Microsoft.EntityFrameworkCore;
using RXNT.API.Data;
using RXNT.API.Models;
using RXNT.API.Repositories;

namespace RXNT.API.Services
{
    public class AppointmentValidationService : IAppointmentValidationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AppointmentValidationService> _logger;

        public AppointmentValidationService(ApplicationDbContext context, ILogger<AppointmentValidationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool IsValid, string ErrorMessage)> ValidateAppointmentAsync(Appointment appointment)
        {
            try
            {
                // Validate Patient exists
                var patientExists = await _context.Patients.AnyAsync(p => p.Id == appointment.PatientId);
                if (!patientExists)
                {
                    return (false, "Patient not found.");
                }

                // Validate Doctor exists
                var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == appointment.DoctorId);
                if (!doctorExists)
                {
                    return (false, "Doctor not found.");
                }

                // Validate Date
                if (appointment.AppointmentDate == default)
                {
                    return (false, "Appointment date is required.");
                }

                // Validate Time
                if (string.IsNullOrWhiteSpace(appointment.AppointmentTime))
                {
                    return (false, "Appointment time is required.");
                }

                if (!ValidateAppointmentDate(appointment.AppointmentDate, appointment.AppointmentTime))
                {
                    return (false, "Appointment date and time cannot be in the past.");
                }

                // Validate Status
                if (!ValidateStatus(appointment.Status))
                {
                    return (false, "Invalid appointment status.");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating appointment");
                return (false, "An error occurred during validation.");
            }
        }

        public bool ValidateAppointmentDate(DateTime appointmentDate, string appointmentTime)
        {
            var appointmentDateTime = appointmentDate.Date;

            if (!string.IsNullOrWhiteSpace(appointmentTime))
            {
                if (TimeSpan.TryParse(appointmentTime, out var time))
                {
                    appointmentDateTime = appointmentDateTime.Add(time);
                }
            }

            return appointmentDateTime >= DateTime.UtcNow;
        }

        public bool ValidateStatus(string status)
        {
            var validStatuses = new[] { "Scheduled", "Completed", "Cancelled" };
            return validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
        }
    }
}
