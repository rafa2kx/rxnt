using Microsoft.Extensions.Logging;
using RXNT.API.Models;
using RXNT.API.DTOs;
using RXNT.API.Repositories;
using RXNT.API.Services;

namespace RXNT.API.Tests
{
    [TestClass]
    public class PatientServiceTests
    {
        private TestPatientRepository _repository = null!;
        private TestPatientValidationService _validationService = null!;
        private ILogger<PatientService> _logger = null!;
        private PatientService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _repository = new TestPatientRepository();
            _validationService = new TestPatientValidationService();
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<PatientService>();
            _service = new PatientService(_repository, _validationService, _logger);
        }

        [TestMethod]
        public async Task GetAllPatientsAsync_ShouldReturnAllPatients()
        {
            // Arrange
            var patients = new List<Patient>
            {
                new Patient { Id = 1, FirstName = "John", LastName = "Doe" },
                new Patient { Id = 2, FirstName = "Jane", LastName = "Smith" }
            };

            _repository.SetPatients(patients);

            // Act
            var result = await _service.GetAllPatientsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task GetPatientByIdAsync_ShouldReturnPatient_WhenExists()
        {
            // Arrange
            var patient = new Patient { Id = 1, FirstName = "John", LastName = "Doe" };
            _repository.SetPatient(patient);

            // Act
            var result = await _service.GetPatientByIdAsync(1);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("John", result.FirstName);
        }

        [TestMethod]
        public async Task CreatePatientAsync_ShouldCreatePatient_WhenValid()
        {
            // Arrange
            var patient = new PatientDto { FirstName = "John", LastName = "Doe", Email = "john@example.com" };
            _validationService.SetValidationResult(true, string.Empty);

            // Act
            var result = await _service.CreatePatientAsync(patient);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsActive);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CreatePatientAsync_ShouldThrowException_WhenValidationFails()
        {
            // Arrange
            var patient = new PatientDto { FirstName = "John", LastName = "Doe" };
            _validationService.SetValidationResult(false, "Validation failed");

            // Act & Assert
            await _service.CreatePatientAsync(patient);
        }

        // Test helper classes
        private class TestPatientRepository : IPatientRepository
        {
            private readonly List<Patient> _patients = new();
            private Patient? _singlePatient;

            public void SetPatients(List<Patient> patients)
            {
                _patients.Clear();
                _patients.AddRange(patients);
            }

            public void SetPatient(Patient patient)
            {
                _singlePatient = patient;
            }

            public Task<IEnumerable<Patient>> GetAllPatientsWithAppointmentsAsync()
            {
                return Task.FromResult<IEnumerable<Patient>>(_patients);
            }

            public Task<Patient?> GetPatientWithAppointmentsAsync(int id)
            {
                return Task.FromResult(_singlePatient);
            }

            public Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null) => Task.FromResult(true);

            // Implement other interface methods as needed
            public Task<Patient?> GetByIdAsync(int id) => Task.FromResult<Patient?>(null);
            public Task<IEnumerable<Patient>> GetAllAsync() => Task.FromResult<IEnumerable<Patient>>(new List<Patient>());
            public Task<IEnumerable<Patient>> FindAsync(System.Linq.Expressions.Expression<Func<Patient, bool>> predicate) => Task.FromResult<IEnumerable<Patient>>(new List<Patient>());
            public Task<Patient?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<Patient, bool>> predicate) => Task.FromResult<Patient?>(null);
            public Task<Patient> AddAsync(Patient entity) => Task.FromResult(entity);
            public Task UpdateAsync(Patient entity) => Task.CompletedTask;
            public Task DeleteAsync(Patient entity) => Task.CompletedTask;
            public Task<bool> ExistsAsync(int id) => Task.FromResult(false);
        }

        private class TestPatientValidationService : IPatientValidationService
        {
            private bool _isValid;
            private string _errorMessage = string.Empty;

            public void SetValidationResult(bool isValid, string errorMessage)
            {
                _isValid = isValid;
                _errorMessage = errorMessage;
            }

            public Task<(bool IsValid, string ErrorMessage)> ValidatePatientAsync(Patient patient, bool isUpdate = false, int? existingId = null)
            {
                return Task.FromResult((_isValid, _errorMessage));
            }

            public bool ValidateDateOfBirth(DateTime dateOfBirth) => true;
            public bool ValidateEmail(string email) => true;
            public bool ValidatePhone(string phone) => true;
            public bool ValidateName(string name) => true;
        }
    }
}
