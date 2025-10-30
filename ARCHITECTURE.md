# RXNT Clinic Manager - Architecture Documentation

## Overview

The RXNT Clinic Manager API follows a layered architecture with Repository Pattern for data access, Service Layer for business logic and validation, and Unit of Work Pattern for transactional operations.

## Architecture Layers

### 1. Repository Layer
**Location**: `api/Repositories/`

The Repository pattern abstracts data access and provides a clean interface for data operations.

#### Generic Repository
- `IRepository<T>`: Generic repository interface with basic CRUD operations
- `Repository<T>`: Base implementation for all repositories

#### Entity-Specific Repositories
- `IPatientRepository` / `PatientRepository`: Patient-specific data access
- `IDoctorRepository` / `DoctorRepository`: Doctor-specific data access
- `IAppointmentRepository` / `AppointmentRepository`: Appointment-specific data access
- `IInvoiceRepository` / `InvoiceRepository`: Invoice-specific data access

#### Benefits
- Separation of data access logic from business logic
- Testability through interface-based design
- Centralized data access patterns

### 2. Data Layer (Entity Framework Core)
**Location**: `api/Data/`

- `ApplicationDbContext`: EF Core DbContext containing DbSets for domain entities and Fluent API configurations.
- Migrations lived under `api/Migrations/` and are applied automatically at application startup.

### 3. Service Layer
**Location**: `api/Services/`

Contains business logic and validation.

#### Validation Services
- `IPatientValidationService` / `PatientValidationService`: Patient validation
  - Name validation (length, format)
  - Email validation (format, uniqueness)
  - Phone validation (format)
  - Date of birth validation
- `IDoctorValidationService` / `DoctorValidationService`: Doctor validation
  - Required fields, specialty, and license number validation
- `IAppointmentValidationService` / `AppointmentValidationService`: Appointment validation
  - Patient existence validation
  - Date/time validation
  - Status validation

#### Business Services
- `IPatientService` / `PatientService`: Patient business logic
- `IAppointmentService` / `AppointmentService`: Appointment business logic

### 4. Controller Layer
**Location**: `api/Controllers/`

Handles HTTP requests and responses. All controllers inherit from `BaseController` which provides:
- Transaction management for write operations
- Centralized exception handling
- Consistent error responses

### 5. Middleware Layer
**Location**: `api/Middleware/`

#### ExceptionHandlingMiddleware
Centralized exception handling with proper HTTP status codes.

## Transaction Management

### How Transactions Work

Transactions are managed in the `BaseController` using Entity Framework's transaction capabilities. All write operations (Create, Update, Delete) in controllers automatically run within transactions.

### Transaction Flow

Transactions are handled using the `ExecuteWithTransactionAsync` method from `BaseController`:
1. **Begin Transaction**: Starts a database transaction
2. **Execute Operation**: Runs the operation within the transaction
3. **Commit**: Automatically commits on success
4. **Rollback**: Automatically rolls back on exception
5. **Dispose**: Ensures proper cleanup

### Example Flow in Controllers

```csharp
return await ExecuteWithTransactionAsync(async () =>
{
    var validation = await _patientService.ValidatePatientAsync(patient);
    if (!validation.IsValid)
        throw new InvalidOperationException(validation.ErrorMessage);

    patient.CreatedDate = DateTime.UtcNow;
    await _context.Patients.AddAsync(patient);
    // SaveChanges occurs within BaseController
    return patient;
});
```

## Validation Flow

### Patient Validation
1. Validate required fields (name, DOB)
2. Validate email format and uniqueness
3. Validate phone format
4. Validate date of birth
5. Return validation result with error message

### Appointment Validation
1. Validate patient exists
2. Validate appointment date/time
3. Validate status
4. Return validation result

## Exception Handling

### Three-Layer Exception Handling

1. **Service Layer**: Business logic exceptions
   - InvalidOperationException for validation errors
   - Proper logging at each level

2. **Controller Layer**: HTTP-specific handling
   - Converts exceptions to appropriate HTTP responses
   - Returns user-friendly error messages

3. **Middleware Layer**: Global exception handler
   - Catches unhandled exceptions
   - Returns consistent error format
   - Logs all errors

### Error Response Format

```json
{
  "message": "Error message",
  "statusCode": 400,
  "timestamp": "2024-01-01T00:00:00Z"
}
```

## Logging

### Logging Levels

- **Information**: Normal operations (GET, POST, PUT, DELETE)
- **Warning**: Validation failures, not found resources
- **Error**: Exceptions and system errors

### Structured Logging

```csharp
_logger.LogInformation("Creating patient: {FirstName} {LastName}", 
    patient.FirstName, patient.LastName);
```

## Dependency Injection

All services are registered in `Program.cs` with appropriate scopes:

```csharp
// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IRepository<Patient>, Repository<Patient>>();
builder.Services.AddScoped<IRepository<Doctor>, Repository<Doctor>>();
builder.Services.AddScoped<IRepository<Appointment>, Repository<Appointment>>();
builder.Services.AddScoped<IRepository<Invoice>, Repository<Invoice>>();

builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

// Services
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
```

## Database Initialization and Migrations

- On startup, the API ensures the database exists and applies EF Core migrations automatically:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}
```

- Migrations are stored under `api/Migrations/` and are part of the API image; no manual intervention is required during container startup.

## Background Processing (Hangfire)

- Hangfire is configured to use SQL Server storage and runs within the API process.
- Dashboard is exposed at `/hangfire`.
- A recurring cleanup job is scheduled daily at 2 AM.

## Data Flow

```
HTTP Request
    ↓
Controller (BaseController - Transaction Management)
    ↓
┌─────────────────────────────────────────────┐
│ Write Operations (Create/Update/Delete)     │
│ - ExecuteInTransactionAsync                 │
│ - HandleException                           │
│ - Direct DbContext access                   │
└─────────────────────────────────────────────┘
    ↓
Service Layer (Business Logic + Validation)
    ↓
Repository (Data Access)
    ↓
Database
```

## Benefits of This Architecture

1. **Separation of Concerns**: Clear separation between data access, business logic, and presentation
2. **Testability**: Interfaces allow easy mocking and unit testing
3. **Maintainability**: Changes in one layer don't affect others
4. **Transactional Integrity**: BaseController ensures all-or-nothing operations for writes
5. **Validation**: Centralized validation logic in validation services
6. **Error Handling**: Centralized exception handling in BaseController
7. **Logging**: Detailed logging for debugging and monitoring
8. **Explicit Transaction Management**: Write operations have clear transaction boundaries

## Key Design Patterns

- **Base Controller Pattern**: Provides transaction management and error handling for all controllers
- **Repository Pattern**: Data access abstraction
- **Service Layer Pattern**: Business logic encapsulation and validation
- **Dependency Injection**: Loose coupling
- **Exception Handling Middleware**: Global exception handler

## Deployment Topology (Docker Compose)

Services defined in `docker-compose.yml`:

- `sqlserver` (SQL Server 2022)
  - Healthcheck uses `/opt/mssql-tools18/bin/sqlcmd -C` to trust the server certificate.
  - Exposes `1433`.
- `api` (ASP.NET Core 8)
  - Depends on `sqlserver` with `condition: service_healthy`.
  - Applies EF Core migrations on startup.
  - Exposes `5000` (maps to container `8080`).
- `app` (Angular + Nginx)
  - Depends on `api`.
  - Exposes `4200`.

## Testing Recommendations

1. **Repository Tests**: Mock DbContext for unit tests
2. **Service Tests**: Mock repositories and validation services
3. **Integration Tests**: Test full request/response cycle
4. **Transaction Tests**: Verify rollback on failures
