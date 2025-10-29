using RXNT.API.Models;
using RXNT.API.DTOs;
using RXNT.API.Repositories;
using RXNT.API.Extensions;

namespace RXNT.API.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAppointmentValidationService _validationService;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IAppointmentValidationService validationService,
            ILogger<AppointmentService> logger)
        {
            _appointmentRepository = appointmentRepository;
            _validationService = validationService;
            _logger = logger;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAppointmentsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all appointments");
                var appointments = await _appointmentRepository.GetAllAppointmentsWithPatientsAsync();
                return appointments.Select(a => a.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all appointments");
                throw;
            }
        }

        public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting appointment with ID: {AppointmentId}", id);
                var appointment = await _appointmentRepository.GetAppointmentWithPatientAsync(id);
                return appointment?.ToDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting appointment with ID: {AppointmentId}", id);
                throw;
            }
        }

        public async Task<AppointmentDto> CreateAppointmentAsync(AppointmentDto appointmentDto)
        {
            _logger.LogInformation("Creating appointment for patient ID: {PatientId}", appointmentDto.PatientId);

            var appointment = appointmentDto.ToEntity();

            // Validate appointment
            var validation = await _validationService.ValidateAppointmentAsync(appointment);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Appointment validation failed: {ErrorMessage}", validation.ErrorMessage);
                throw new InvalidOperationException(validation.ErrorMessage);
            }

            // Business logic: Set created date
            appointment.CreatedDate = DateTime.UtcNow;

            // Call repository for data access
            await _appointmentRepository.AddAsync(appointment);

            _logger.LogInformation("Appointment created successfully");
            return appointment.ToDto();
        }

        public async Task<AppointmentDto?> UpdateAppointmentAsync(int id, AppointmentDto appointmentDto)
        {
            _logger.LogInformation("Updating appointment with ID: {AppointmentId}", id);

            var appointment = appointmentDto.ToEntity();

            // Validate appointment
            var validation = await _validationService.ValidateAppointmentAsync(appointment);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Appointment validation failed: {ErrorMessage}", validation.ErrorMessage);
                throw new InvalidOperationException(validation.ErrorMessage);
            }

            var existingAppointment = await _appointmentRepository.GetByIdAsync(id);
            if (existingAppointment == null)
            {
                _logger.LogWarning("Appointment not found with ID: {AppointmentId}", id);
                return null;
            }

            // Business logic: Update properties
            existingAppointment.PatientId = appointment.PatientId;
            existingAppointment.DoctorId = appointment.DoctorId;
            existingAppointment.AppointmentDate = appointment.AppointmentDate;
            existingAppointment.AppointmentTime = appointment.AppointmentTime;
            existingAppointment.Reason = appointment.Reason;
            existingAppointment.Notes = appointment.Notes;
            existingAppointment.Status = appointment.Status;
            existingAppointment.UpdatedDate = DateTime.UtcNow;

            // Call repository for data access
            await _appointmentRepository.UpdateAsync(existingAppointment);

            _logger.LogInformation("Appointment updated successfully");
            return existingAppointment.ToDto();
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            _logger.LogInformation("Deleting appointment with ID: {AppointmentId}", id);

            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null)
            {
                _logger.LogWarning("Appointment not found with ID: {AppointmentId}", id);
                return false;
            }

            // Call repository for data access
            await _appointmentRepository.DeleteAsync(appointment);

            _logger.LogInformation("Appointment deleted successfully");
            return true;
        }

        public async Task<(bool IsValid, string ErrorMessage)> ValidateAppointmentAsync(Appointment appointment)
        {
            return await _validationService.ValidateAppointmentAsync(appointment);
        }
    }
}
