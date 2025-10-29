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
- `IAppointmentRepository` / `AppointmentRepository`: Appointment-specific data access

#### Benefits
- Separation of data access logic from business logic
- Testability through interface-based design
- Centralized data access patterns

### 2. Unit of Work Pattern
**Location**: `api/UnitOfWork/`

Manages transactions across multiple repositories and ensures data consistency.

- `IUnitOfWork`: Interface defining transaction boundaries
- `UnitOfWork`: Implementation managing database transactions

#### Features
- **Transactional Operations**: Ensures all-or-nothing database commits
- **Rollback Support**: Automatic rollback on exceptions
- **Centralized Save**: Single point for SaveChanges operations

### 3. Service Layer
**Location**: `api/Services/`

Contains business logic and validation.

#### Validation Services
- `IPatientValidationService` / `PatientValidationService`: Patient validation
  - Name validation (length, format)
  - Email validation (format, uniqueness)
  - Phone validation (format)
  - Date of birth validation
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

Transactions are handled using the `ExecuteInTransactionAsync` method from `BaseController`:
1. **Begin Transaction**: Starts a database transaction
2. **Execute Operation**: Runs the operation within the transaction
3. **Commit**: Automatically commits on success
4. **Rollback**: Automatically rolls back on exception
5. **Dispose**: Ensures proper cleanup

### Example Flow in Controllers

```csharp
var createdPatient = await ExecuteInTransactionAsync(async (context) =>
{
    // Validation
    var validation = await _patientService.ValidatePatientAsync(patient);
    if (!validation.IsValid)
        throw new InvalidOperationException(validation.ErrorMessage);

    // Database operations
    patient.CreatedDate = DateTime.UtcNow;
    await context.Patients.AddAsync(patient);
    await context.SaveChangesAsync();

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
// Scoped services (one per request)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
```

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
Service Layer (Business Logic + Validation for Reads)
    ↓
Repository (Data Access for Reads)
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
- **Repository Pattern**: Data access abstraction (for read operations)
- **Service Layer Pattern**: Business logic encapsulation and validation
- **Dependency Injection**: Loose coupling
- **Exception Handling Middleware**: Global exception handler
- **Template Method Pattern**: BaseController defines transaction template

## Testing Recommendations

1. **Repository Tests**: Mock DbContext for unit tests
2. **Service Tests**: Mock repositories and validation services
3. **Integration Tests**: Test full request/response cycle
4. **Transaction Tests**: Verify rollback on failures
