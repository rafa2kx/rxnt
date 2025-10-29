using Microsoft.EntityFrameworkCore;
using RXNT.API.Data;
using RXNT.API.Models;
using System.Text.RegularExpressions;

namespace RXNT.API.Services
{
    public class DoctorValidationService : IDoctorValidationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DoctorValidationService> _logger;

        public DoctorValidationService(
            ApplicationDbContext context,
            ILogger<DoctorValidationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool IsValid, string ErrorMessage)> ValidateDoctorAsync(Doctor doctor, bool isUpdate = false, int? existingId = null)
        {
            // Validate First Name
            if (string.IsNullOrWhiteSpace(doctor.FirstName) || doctor.FirstName.Length < 2 || doctor.FirstName.Length > 100)
            {
                return (false, "First name is required and must be between 2 and 100 characters.");
            }

            // Validate Last Name
            if (string.IsNullOrWhiteSpace(doctor.LastName) || doctor.LastName.Length < 2 || doctor.LastName.Length > 100)
            {
                return (false, "Last name is required and must be between 2 and 100 characters.");
            }

            // Validate Email if provided
            if (!string.IsNullOrWhiteSpace(doctor.Email))
            {
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                if (!emailRegex.IsMatch(doctor.Email))
                {
                    return (false, "Invalid email format.");
                }

                if (doctor.Email.Length > 200)
                {
                    return (false, "Email must not exceed 200 characters.");
                }

                // Check email uniqueness
                var existingDoctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.Email.ToLower() == doctor.Email.ToLower() && (!isUpdate || d.Id != existingId));
                
                if (existingDoctor != null)
                {
                    return (false, "Email already exists in the system.");
                }
            }

            // Validate Phone if provided
            if (!string.IsNullOrWhiteSpace(doctor.Phone))
            {
                var phoneRegex = new Regex(@"^[\d\s\(\)\-\+\.]+$");
                if (!phoneRegex.IsMatch(doctor.Phone))
                {
                    return (false, "Invalid phone number format.");
                }

                if (doctor.Phone.Length > 20)
                {
                    return (false, "Phone number must not exceed 20 characters.");
                }
            }

            // Validate License Number
            if (string.IsNullOrWhiteSpace(doctor.LicenseNumber))
            {
                return (false, "License number is required.");
            }

            if (doctor.LicenseNumber.Length > 50)
            {
                return (false, "License number must not exceed 50 characters.");
            }

            // Check license number uniqueness
            var existingLicenseDoctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.LicenseNumber.ToLower() == doctor.LicenseNumber.ToLower() && (!isUpdate || d.Id != existingId));
            
            if (existingLicenseDoctor != null)
            {
                return (false, "License number already exists in the system.");
            }

            // Validate Specialty
            if (string.IsNullOrWhiteSpace(doctor.Specialty))
            {
                return (false, "Specialty is required.");
            }

            if (doctor.Specialty.Length > 200)
            {
                return (false, "Specialty must not exceed 200 characters.");
            }

            return (true, string.Empty);
        }
    }
}
