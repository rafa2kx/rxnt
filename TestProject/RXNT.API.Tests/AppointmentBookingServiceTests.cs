using Microsoft.Extensions.Logging;
using RXNT.API.Models;
using RXNT.API.Repositories;
using RXNT.API.Services;

namespace RXNT.API.Tests
{
    [TestClass]
    public class AppointmentBookingServiceTests
    {
        private TestAppointmentValidationService _validationService = null!;
        private TestAppointmentRepository _appointmentRepository = null!;
        private TestInvoiceRepository _invoiceRepository = null!;
        private ILogger<AppointmentBookingService> _logger = null!;
        private AppointmentBookingService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _validationService = new TestAppointmentValidationService();
            _appointmentRepository = new TestAppointmentRepository();
            _invoiceRepository = new TestInvoiceRepository();
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AppointmentBookingService>();
            
            _service = new AppointmentBookingService(
                _validationService,
                _appointmentRepository,
                _invoiceRepository,
                _logger);
        }

        [TestMethod]
        public async Task ScheduleAppointmentWithInvoiceAsync_ShouldCreateBothAppointmentAndInvoice_WhenValid()
        {
            // Arrange
            var appointmentDate = DateTime.UtcNow.AddDays(1);
            _validationService.SetValidationResult(true, string.Empty);
            _invoiceRepository.SetInvoiceNumber("INV-20250126-0001");

            // Act
            var result = await _service.ScheduleAppointmentWithInvoiceAsync(1, 1, appointmentDate, "10:00", "Checkup", 100m);

            // Assert
            Assert.IsNotNull(result.Appointment);
            Assert.IsNotNull(result.Invoice);
            Assert.AreEqual("Pending", result.Invoice.Status);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ScheduleAppointmentWithInvoiceAsync_ShouldThrowException_WhenValidationFails()
        {
            // Arrange
            var appointmentDate = DateTime.UtcNow.AddDays(1);
            _validationService.SetValidationResult(false, "Invalid appointment");

            // Act & Assert
            await _service.ScheduleAppointmentWithInvoiceAsync(1, 1, appointmentDate, "10:00", "Checkup", 100m);
        }

        // Test helper classes
        private class TestAppointmentValidationService : IAppointmentValidationService
        {
            private bool _isValid = true;
            private string _errorMessage = string.Empty;

            public void SetValidationResult(bool isValid, string errorMessage)
            {
                _isValid = isValid;
                _errorMessage = errorMessage;
            }

            public Task<(bool IsValid, string ErrorMessage)> ValidateAppointmentAsync(Appointment appointment)
            {
                return Task.FromResult((_isValid, _errorMessage));
            }

            public bool ValidateAppointmentDate(DateTime appointmentDate, string appointmentTime)
            {
                return true;
            }

            public bool ValidateStatus(string status)
            {
                return true;
            }
        }

        private class TestAppointmentRepository : IAppointmentRepository
        {
            public Task<Appointment> AddAsync(Appointment entity)
            {
                entity.Id = 1; // Simulate ID assignment
                return Task.FromResult(entity);
            }

            // Implement other interface methods as needed
            public Task<IEnumerable<Appointment>> GetAllAppointmentsWithPatientsAsync() => Task.FromResult<IEnumerable<Appointment>>(new List<Appointment>());
            public Task<Appointment?> GetAppointmentWithPatientAsync(int id) => Task.FromResult<Appointment?>(null);
            public Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId) => Task.FromResult<IEnumerable<Appointment>>(new List<Appointment>());
            public Task<Appointment?> GetByIdAsync(int id) => Task.FromResult<Appointment?>(null);
            public Task<IEnumerable<Appointment>> GetAllAsync() => Task.FromResult<IEnumerable<Appointment>>(new List<Appointment>());
            public Task<IEnumerable<Appointment>> FindAsync(System.Linq.Expressions.Expression<Func<Appointment, bool>> predicate) => Task.FromResult<IEnumerable<Appointment>>(new List<Appointment>());
            public Task<Appointment?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<Appointment, bool>> predicate) => Task.FromResult<Appointment?>(null);
            public Task UpdateAsync(Appointment entity) => Task.CompletedTask;
            public Task DeleteAsync(Appointment entity) => Task.CompletedTask;
            public Task<bool> ExistsAsync(int id) => Task.FromResult(false);
        }

        private class TestInvoiceRepository : IInvoiceRepository
        {
            private string _invoiceNumber = "INV-20250126-0001";
            private int _invoiceCounter = 1;

            public void SetInvoiceNumber(string invoiceNumber)
            {
                _invoiceNumber = invoiceNumber;
            }

            public Task<string> GenerateInvoiceNumberAsync()
            {
                var number = $"INV-{DateTime.Now:yyyyMMdd}-{_invoiceCounter:D4}";
                _invoiceCounter++;
                return Task.FromResult(number);
            }

            public Task<Invoice> AddAsync(Invoice entity)
            {
                entity.Id = _invoiceCounter;
                entity.InvoiceNumber = _invoiceNumber;
                return Task.FromResult(entity);
            }

            // Implement other interface methods as needed
            public Task<IEnumerable<Invoice>> GetAllInvoicesWithAppointmentsAsync() => Task.FromResult<IEnumerable<Invoice>>(new List<Invoice>());
            public Task<Invoice?> GetInvoiceWithAppointmentAsync(int id) => Task.FromResult<Invoice?>(null);
            public Task<Invoice?> GetByIdAsync(int id) => Task.FromResult<Invoice?>(null);
            public Task<IEnumerable<Invoice>> GetAllAsync() => Task.FromResult<IEnumerable<Invoice>>(new List<Invoice>());
            public Task<IEnumerable<Invoice>> FindAsync(System.Linq.Expressions.Expression<Func<Invoice, bool>> predicate) => Task.FromResult<IEnumerable<Invoice>>(new List<Invoice>());
            public Task<Invoice?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<Invoice, bool>> predicate) => Task.FromResult<Invoice?>(null);
            public Task UpdateAsync(Invoice entity) => Task.CompletedTask;
            public Task DeleteAsync(Invoice entity) => Task.CompletedTask;
            public Task<bool> ExistsAsync(int id) => Task.FromResult(false);
        }
    }
}
