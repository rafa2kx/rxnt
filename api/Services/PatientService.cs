using RXNT.API.Models;
using RXNT.API.DTOs;
using RXNT.API.Repositories;
using RXNT.API.Extensions;

namespace RXNT.API.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IPatientValidationService _validationService;
        private readonly ILogger<PatientService> _logger;

        public PatientService(
            IPatientRepository patientRepository,
            IPatientValidationService validationService,
            ILogger<PatientService> logger)
        {
            _patientRepository = patientRepository;
            _validationService = validationService;
            _logger = logger;
        }

        public async Task<IEnumerable<PatientDto>> GetAllPatientsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all patients");
                var patients = await _patientRepository.GetAllPatientsWithAppointmentsAsync();
                return patients.Select(p => p.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all patients");
                throw;
            }
        }

        public async Task<PatientDto?> GetPatientByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting patient with ID: {PatientId}", id);
                var patient = await _patientRepository.GetPatientWithAppointmentsAsync(id);
                return patient?.ToDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting patient with ID: {PatientId}", id);
                throw;
            }
        }

        public async Task<PatientDto> CreatePatientAsync(PatientDto patientDto)
        {
            _logger.LogInformation("Creating patient: {FirstName} {LastName}", patientDto.FirstName, patientDto.LastName);

            var patient = patientDto.ToEntity();

            // Validate patient
            var validation = await _validationService.ValidatePatientAsync(patient);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Patient validation failed: {ErrorMessage}", validation.ErrorMessage);
                throw new InvalidOperationException(validation.ErrorMessage);
            }

            // Business logic: Set default values
            patient.CreatedDate = DateTime.UtcNow;
            patient.IsActive = true;

            // Call repository for data access
            await _patientRepository.AddAsync(patient);

            _logger.LogInformation("Patient created successfully");
            return patient.ToDto();
        }

        public async Task<PatientDto?> UpdatePatientAsync(int id, PatientDto patientDto)
        {
            _logger.LogInformation("Updating patient with ID: {PatientId}", id);

            var patient = patientDto.ToEntity();

            // Validate patient
            var validation = await _validationService.ValidatePatientAsync(patient, isUpdate: true, existingId: id);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Patient validation failed: {ErrorMessage}", validation.ErrorMessage);
                throw new InvalidOperationException(validation.ErrorMessage);
            }

            var existingPatient = await _patientRepository.GetByIdAsync(id);
            if (existingPatient == null)
            {
                _logger.LogWarning("Patient not found with ID: {PatientId}", id);
                return null;
            }

            // Business logic: Update properties
            existingPatient.FirstName = patient.FirstName;
            existingPatient.LastName = patient.LastName;
            existingPatient.DateOfBirth = patient.DateOfBirth;
            existingPatient.Email = patient.Email;
            existingPatient.Phone = patient.Phone;
            existingPatient.Address = patient.Address;
            existingPatient.Gender = patient.Gender;
            existingPatient.IsActive = patient.IsActive;
            existingPatient.UpdatedDate = DateTime.UtcNow;

            // Call repository for data access
            await _patientRepository.UpdateAsync(existingPatient);

            _logger.LogInformation("Patient updated successfully");
            return existingPatient.ToDto();
        }

        public async Task<bool> DeletePatientAsync(int id)
        {
            _logger.LogInformation("Deleting patient with ID: {PatientId}", id);

            var patient = await _patientRepository.GetPatientWithAppointmentsAsync(id);
            if (patient == null)
            {
                _logger.LogWarning("Patient not found with ID: {PatientId}", id);
                return false;
            }

            // Business rule: Cannot delete patient if they have associated appointments
            if (patient.Appointments != null && patient.Appointments.Any())
            {
                _logger.LogWarning("Cannot delete patient with ID: {PatientId} because they have associated appointments.", id);
                throw new InvalidOperationException($"Cannot delete patient with ID {id} because they have associated appointments.");
            }

            // Call repository for data access
            await _patientRepository.DeleteAsync(patient);

            _logger.LogInformation("Patient deleted successfully");
            return true;
        }

        public async Task<(bool IsValid, string ErrorMessage)> ValidatePatientAsync(Patient patient, bool isUpdate = false, int? existingId = null)
        {
            return await _validationService.ValidatePatientAsync(patient, isUpdate, existingId);
        }
    }
}
