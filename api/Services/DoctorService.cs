using RXNT.API.Models;
using RXNT.API.DTOs;
using RXNT.API.Repositories;
using RXNT.API.Extensions;

namespace RXNT.API.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IDoctorValidationService _validationService;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ILogger<DoctorService> _logger;

        public DoctorService(
            IDoctorRepository doctorRepository,
            IDoctorValidationService validationService,
            IAppointmentRepository appointmentRepository,
            ILogger<DoctorService> logger)
        {
            _doctorRepository = doctorRepository;
            _validationService = validationService;
            _appointmentRepository = appointmentRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<DoctorDto>> GetAllDoctorsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all doctors");
                var doctors = await _doctorRepository.GetAllDoctorsWithAppointmentsAsync();
                return doctors.Select(d => d.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all doctors");
                throw;
            }
        }

        public async Task<DoctorDto?> GetDoctorByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting doctor with ID: {DoctorId}", id);
                var doctor = await _doctorRepository.GetDoctorWithAppointmentsAsync(id);
                return doctor?.ToDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting doctor with ID: {DoctorId}", id);
                throw;
            }
        }

        public async Task<DoctorDto> CreateDoctorAsync(DoctorDto doctorDto)
        {
            _logger.LogInformation("Creating doctor: {FirstName} {LastName}", doctorDto.FirstName, doctorDto.LastName);

            var doctor = doctorDto.ToEntity();

            // Validate doctor
            var validation = await _validationService.ValidateDoctorAsync(doctor);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Doctor validation failed: {ErrorMessage}", validation.ErrorMessage);
                throw new InvalidOperationException(validation.ErrorMessage);
            }

            // Business logic: Set default values
            doctor.CreatedDate = DateTime.UtcNow;
            doctor.IsActive = true;

            // Call repository for data access
            await _doctorRepository.AddAsync(doctor);

            _logger.LogInformation("Doctor created successfully");
            return doctor.ToDto();
        }

        public async Task<DoctorDto?> UpdateDoctorAsync(int id, DoctorDto doctorDto)
        {
            _logger.LogInformation("Updating doctor with ID: {DoctorId}", id);

            var doctor = doctorDto.ToEntity();

            // Validate doctor
            var validation = await _validationService.ValidateDoctorAsync(doctor, isUpdate: true, existingId: id);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Doctor validation failed: {ErrorMessage}", validation.ErrorMessage);
                throw new InvalidOperationException(validation.ErrorMessage);
            }

            var existingDoctor = await _doctorRepository.GetByIdAsync(id);
            if (existingDoctor == null)
            {
                _logger.LogWarning("Doctor not found with ID: {DoctorId}", id);
                return null;
            }

            // Business logic: Update properties
            existingDoctor.FirstName = doctor.FirstName;
            existingDoctor.LastName = doctor.LastName;
            existingDoctor.Specialty = doctor.Specialty;
            existingDoctor.Email = doctor.Email;
            existingDoctor.Phone = doctor.Phone;
            existingDoctor.LicenseNumber = doctor.LicenseNumber;
            existingDoctor.IsActive = doctor.IsActive;
            existingDoctor.UpdatedDate = DateTime.UtcNow;

            // Call repository for data access
            await _doctorRepository.UpdateAsync(existingDoctor);

            _logger.LogInformation("Doctor updated successfully");
            return existingDoctor.ToDto();
        }

        public async Task<bool> DeleteDoctorAsync(int id)
        {
            _logger.LogInformation("Deleting doctor with ID: {DoctorId}", id);

            var doctor = await _doctorRepository.GetDoctorWithAppointmentsAsync(id);
            if (doctor == null)
            {
                _logger.LogWarning("Doctor not found with ID: {DoctorId}", id);
                return false;
            }

            // Business rule: Cannot delete doctor if they have associated appointments
            if (doctor.Appointments != null && doctor.Appointments.Any())
            {
                _logger.LogWarning("Cannot delete doctor with ID: {DoctorId} because they have associated appointments.", id);
                throw new InvalidOperationException($"Cannot delete doctor with ID {id} because they have associated appointments.");
            }

            // Call repository for data access
            await _doctorRepository.DeleteAsync(doctor);

            _logger.LogInformation("Doctor deleted successfully");
            return true;
        }

        public async Task<IEnumerable<AppointmentDto>> GetDoctorScheduleAsync(int doctorId)
        {
            _logger.LogInformation("Getting schedule for doctor ID: {DoctorId}", doctorId);
            var appointments = await _appointmentRepository.GetAppointmentsByDoctorAsync(doctorId);
            return appointments.Select(a => a.ToDto());
        }

        public async Task<(bool IsValid, string ErrorMessage)> ValidateDoctorAsync(Doctor doctor, bool isUpdate = false, int? existingId = null)
        {
            return await _validationService.ValidateDoctorAsync(doctor, isUpdate, existingId);
        }
    }
}
