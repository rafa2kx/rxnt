# Migrations and Unit Tests

## Database Migrations

### Migration Generated
- **Name**: `AddDoctorsAndInvoices`
- **Location**: `api/Migrations/`
- **Description**: Adds Doctor and Invoice entities to the database, creates all necessary indexes and foreign key relationships

### Generated Files
- `[timestamp]_AddDoctorsAndInvoices.cs` - Migration file
- `ApplicationDbContextModelSnapshot.cs` - Snapshot of current model state

### To Apply Migration
```bash
cd api
dotnet ef database update
```

### To Rollback Migration
```bash
cd api
dotnet ef migrations remove
```

## Unit Tests

### Test Project Structure
```
TestProject/
└── RXNT.API.Tests/
    ├── RXNT.API.Tests.csproj
    ├── PatientServiceTests.cs
    └── AppointmentBookingServiceTests.cs
```

### Test Files Created

#### 1. PatientServiceTests.cs
Tests for `PatientService` including:
- `GetAllPatientsAsync_ShouldReturnAllPatients` - Tests retrieving all patients
- `GetPatientByIdAsync_ShouldReturnPatient_WhenExists` - Tests retrieving by ID
- `CreatePatientAsync_ShouldCreatePatient_WhenValid` - Tests patient creation
- `CreatePatientAsync_ShouldThrowException_WhenValidationFails` - Tests validation

#### 2. AppointmentBookingServiceTests.cs
Tests for `AppointmentBookingService` including:
- `ScheduleAppointmentWithInvoiceAsync_ShouldCreateBothAppointmentAndInvoice_WhenValid` - Tests transactional booking
- `ScheduleAppointmentWithInvoiceAsync_ShouldThrowException_WhenValidationFails` - Tests validation failure

### Test Dependencies
- **MSTest** - Built-in Microsoft testing framework
- **Manual test implementations** - Concrete test doubles without external libraries

### Running Tests

#### Run All Tests
```bash
cd TestProject/RXNT.API.Tests
dotnet test
```

#### Run Specific Test Class
```bash
dotnet test --filter "FullyQualifiedName~PatientServiceTests"
```

#### Run with Verbose Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Test Coverage

#### Services Tested
- ✅ PatientService
- ✅ AppointmentBookingService

#### Services That Could Be Added
- DoctorService
- AppointmentService
- InvoiceService
- Validation Services (PatientValidationService, AppointmentValidationService, DoctorValidationService)

### Test Implementation Approach

Tests use **manual test doubles** (concrete implementations) instead of mocking frameworks. This approach:
- ✅ No external dependencies
- ✅ Uses only MSTest framework
- ✅ Clear and explicit test behavior
- ✅ Easy to understand and maintain

Example template for a new service test:

```csharp
using Microsoft.Extensions.Logging;
using RXNT.API.Models;
using RXNT.API.Repositories;
using RXNT.API.Services;

namespace RXNT.API.Tests
{
    [TestClass]
    public class YourServiceTests
    {
        private TestRepository _repository = null!;
        private ILogger<YourService> _logger = null!;
        private YourService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _repository = new TestRepository();
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<YourService>();
            _service = new YourService(_repository, _logger);
        }

        [TestMethod]
        public async Task YourMethod_ShouldDoSomething_WhenCondition()
        {
            // Arrange
            _repository.SetData(expectedValue);

            // Act
            var result = await _service.YourMethodAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedValue, result.Property);
        }

        // Test helper class
        private class TestRepository : IRepository
        {
            private object? _data;

            public void SetData(object data) => _data = data;

            public Task<object?> GetAsync() => Task.FromResult(_data);
            // Implement other interface methods...
        }
    }
}
```

## Integration Tests (Future Enhancement)

For integration tests that test the full stack:

1. Create a separate project: `RXNT.API.IntegrationTests`
2. Use `WebApplicationFactory` for testing controllers
3. Use in-memory database or test database
4. Test end-to-end scenarios

Example structure:
```csharp
public class AppointmentsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AppointmentsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task PostAppointment_ShouldCreateAppointmentAndInvoice()
    {
        // Test implementation
    }
}
```

## Best Practices

1. **Arrange-Act-Assert Pattern**: Structure tests clearly
2. **Manual Test Doubles**: Use concrete implementations instead of mocks
3. **Test Edge Cases**: Test both success and failure scenarios
4. **Meaningful Test Names**: Use descriptive test method names
5. **Isolated Tests**: Each test should be independent
6. **Use MSTest Assertions**: Assert.IsNotNull, Assert.AreEqual, etc.
7. **No External Libraries**: Keep tests simple and dependency-free

## CI/CD Integration

To run tests in CI/CD pipeline:

```yaml
# Example GitHub Actions
- name: Run Tests
  run: dotnet test --no-build --verbosity normal

- name: Generate Coverage Report
  run: dotnet test --collect:"XPlat Code Coverage"
```

## Notes

- Migration successfully created for Doctors and Invoices entities
- Unit test infrastructure using MSTest (no external libraries)
- Test files created using manual test doubles (concrete implementations)
- 7 tests passing for PatientService and AppointmentBookingService
- Additional test files can be added following the same pattern
- Consider adding integration tests for end-to-end testing

## Test Results

```
Test summary: total: 7, failed: 0, succeeded: 7, skipped: 0
```

All tests are passing! ✅
