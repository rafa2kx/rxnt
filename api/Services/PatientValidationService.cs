using Microsoft.EntityFrameworkCore;
using RXNT.API.Data;
using RXNT.API.Models;
using RXNT.API.Repositories;
using System.Text.RegularExpressions;

namespace RXNT.API.Services
{
    public class PatientValidationService : IPatientValidationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PatientValidationService> _logger;

        public PatientValidationService(ApplicationDbContext context, ILogger<PatientValidationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool IsValid, string ErrorMessage)> ValidatePatientAsync(Patient patient, bool isUpdate = false, int? existingId = null)
        {
            try
            {
                // Validate First Name
                if (!ValidateName(patient.FirstName))
                {
                    return (false, "First name is required and must be between 2 and 100 characters.");
                }

                // Validate Last Name
                if (!ValidateName(patient.LastName))
                {
                    return (false, "Last name is required and must be between 2 and 100 characters.");
                }

                // Validate Email
                if (!string.IsNullOrWhiteSpace(patient.Email) && !ValidateEmail(patient.Email))
                {
                    return (false, "Invalid email format.");
                }

                // Check email uniqueness
                if (!string.IsNullOrWhiteSpace(patient.Email))
                {
                    var emailExists = _context.Patients
                        .Where(p => p.Email.ToLower() == patient.Email.ToLower());
                    
                    if (existingId.HasValue)
                    {
                        emailExists = emailExists.Where(p => p.Id != existingId.Value);
                    }
                    
                    if (await emailExists.AnyAsync())
                    {
                        return (false, "Email already exists in the system.");
                    }
                }

                // Validate Phone
                if (!string.IsNullOrWhiteSpace(patient.Phone) && !ValidatePhone(patient.Phone))
                {
                    return (false, "Invalid phone number format.");
                }

                // Validate Date of Birth
                if (patient.DateOfBirth == default)
                {
                    return (false, "Date of birth is required.");
                }

                if (!ValidateDateOfBirth(patient.DateOfBirth))
                {
                    return (false, "Date of birth cannot be in the future.");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating patient");
                return (false, "An error occurred during validation.");
            }
        }

        public bool ValidateDateOfBirth(DateTime dateOfBirth)
        {
            return dateOfBirth <= DateTime.UtcNow;
        }

        public bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                return regex.IsMatch(email) && email.Length <= 200;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Allow various phone formats: (123) 456-7890, 123-456-7890, 1234567890, +1-123-456-7890
            var regex = new Regex(@"^[\+]?[(]?[0-9]{1,4}[)]?[-\s\.]?[(]?[0-9]{1,4}[)]?[-\s\.]?[0-9]{1,9}$");
            return regex.IsMatch(phone) && phone.Length <= 20;
        }

        public bool ValidateName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Length >= 2 && name.Length <= 100;
        }
    }
}
