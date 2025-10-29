# Validation Rules

## Patient Validation

### Required Fields
- **FirstName**: Required, 2-100 characters
- **LastName**: Required, 2-100 characters
- **DateOfBirth**: Required, cannot be in the future
- **Gender**: Required (Male, Female, Other)

### Optional Fields
- **Email**: Optional, but if provided must be:
  - Valid email format
  - Unique across all patients
  - Maximum 200 characters
- **Phone**: Optional, but if provided must be:
  - Valid phone format
  - Maximum 20 characters
  - Supports formats: (123) 456-7890, 123-456-7890, 1234567890, +1-123-456-7890
- **Address**: Optional, no specific validation

### Business Rules
- Email must be unique (case-insensitive)
- Cannot delete a patient with existing appointments
- Date of birth cannot be in the future

## Appointment Validation

### Required Fields
- **PatientId**: Required, must exist in database
- **AppointmentDate**: Required
- **AppointmentTime**: Required
- **Status**: Required, must be one of: Scheduled, Completed, Cancelled

### Validation Rules
- Patient must exist in the system
- Appointment date and time cannot be in the past
- Status must be one of the valid values

### Business Rules
- Cannot schedule appointments in the past
- Patient must exist before creating appointment

## Error Messages

All validation errors return user-friendly messages:
- "First name is required and must be between 2 and 100 characters."
- "Last name is required and must be between 2 and 100 characters."
- "Invalid email format."
- "Email already exists in the system."
- "Invalid phone number format."
- "Date of birth is required."
- "Date of birth cannot be in the future."
- "Patient not found."
- "Appointment date and time cannot be in the past."
- "Invalid appointment status."

## Implementation

Validation is implemented in dedicated validation services:
- `PatientValidationService`: Handles all patient validation logic
- `AppointmentValidationService`: Handles all appointment validation logic

Both services are called before any database operations to ensure data integrity.
