# BaseController Pattern

## Overview

The `BaseController` provides a foundation for all controllers, wrapping service calls with database transactions and handling exceptions consistently.

## Architecture Flow

```
Request → Controller → Transaction Wrapper → Service → Validation → Repository → DB
```

### Flow Details

1. **Controller**: Receives HTTP request
2. **BaseController**: Wraps service call in a transaction
3. **Service**: Validates data and applies business logic
4. **Repository**: Performs database operations
5. **Transaction**: Commits on success or rolls back on error

## Key Features

### 1. Transaction Management

All service calls that modify data are automatically wrapped in database transactions using `ExecuteWithTransactionAsync`.

**Benefits:**
- Automatic rollback on errors
- Guaranteed data consistency
- Proper transaction disposal
- Centralized transaction handling

### 2. Error Handling

Centralized exception handling through the `HandleException` method provides consistent error responses.

**Exception Types:**
- `InvalidOperationException` → 400 Bad Request
- `KeyNotFoundException` → 404 Not Found
- `DbUpdateException` → 500 Internal Server Error
- Other exceptions → 500 Internal Server Error

### 3. Logging

All operations are logged for debugging and monitoring purposes.

## Usage

### Inheriting from BaseController

```csharp
[Route("api/[controller]")]
public class PatientsController : BaseController
{
    private readonly IPatientService _patientService;

    public PatientsController(
        IPatientService patientService,
        ApplicationDbContext context,
        ILogger<BaseController> logger)
        : base(context, logger)
    {
        _patientService = patientService;
    }
}
```

### Creating Resources

```csharp
[HttpPost]
public async Task<IActionResult> CreatePatient(Patient patient)
{
    return await ExecuteWithTransactionAsync(async () =>
    {
        var createdPatient = await _patientService.CreatePatientAsync(patient);
        return CreatedAtAction(nameof(GetPatient), new { id = createdPatient.Id }, createdPatient);
    });
}
```

### Updating Resources

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdatePatient(int id, Patient patient)
{
    return await ExecuteWithTransactionAsync(async () =>
    {
        var updatedPatient = await _patientService.UpdatePatientAsync(id, patient);
        if (updatedPatient == null)
            throw new KeyNotFoundException($"Patient with ID {id} not found");

        return updatedPatient;
    });
}
```

### Deleting Resources

```csharp
[HttpDelete("{id}")]
public async Task<IActionResult> DeletePatient(int id)
{
    return await ExecuteWithTransactionAsync(async () =>
    {
        var result = await _patientService.DeletePatientAsync(id);
        if (!result)
            throw new KeyNotFoundException($"Patient with ID {id} not found");
    });
}
```

## Transaction Flow

```
1. Controller calls ExecuteWithTransactionAsync
   ↓
2. BaseController begins transaction
   ↓
3. Service executes (validation, business logic)
   ↓
4. Repository executes (database operations)
   ↓
5. SaveChanges called within transaction
   ├─ Success → Commit transaction
   └─ Exception → Rollback transaction
   ↓
6. Transaction disposed
   ↓
7. Return result or handle exception
```

## Layer Responsibilities

### Controller Layer
- Handle HTTP requests/responses
- Wrap service calls with transactions
- Convert exceptions to HTTP responses

### Service Layer
- Validate input data
- Apply business logic
- Call repositories for data access
- No direct database access

### Repository Layer
- Perform database operations
- Call SaveChanges within transaction context
- Abstract database details

### BaseController
- Manage transactions globally
- Handle exceptions consistently
- Log operations

## Benefits

### 1. **Separation of Concerns**
- Clear boundaries between layers
- Each layer has a single responsibility
- Easy to test and maintain

### 2. **Consistency**
- All write operations follow the same pattern
- Uniform error handling across all controllers
- Consistent transaction management

### 3. **Safety**
- Automatic rollback on errors
- Proper resource disposal
- No transaction leaks

### 4. **Maintainability**
- Single place for transaction logic
- Easy to modify transaction behavior
- Clear separation of concerns

## Best Practices

1. **Use transactions for writes**: Only wrap operations that modify data
2. **Throw appropriate exceptions**: Use InvalidOperationException for validation errors
3. **Don't access DbContext in controllers**: Let services handle all data access
4. **Keep controllers thin**: Controllers should only coordinate between layers

## Summary

The BaseController pattern provides:
- ✅ Automatic transaction management
- ✅ Centralized error handling
- ✅ Consistent error responses
- ✅ Proper resource cleanup
- ✅ Simplified controller code
- ✅ Better maintainability
- ✅ Clear separation of concerns
