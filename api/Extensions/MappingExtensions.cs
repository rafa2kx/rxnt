using RXNT.API.Models;
using RXNT.API.DTOs;

namespace RXNT.API.Extensions
{
    public static class MappingExtensions
    {
        // Patient mappings
        public static PatientDto ToDto(this Patient patient)
        {
            return new PatientDto
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateOfBirth = patient.DateOfBirth,
                Email = patient.Email,
                Phone = patient.Phone,
                Address = patient.Address,
                Gender = patient.Gender,
                CreatedDate = patient.CreatedDate,
                UpdatedDate = patient.UpdatedDate,
                IsActive = patient.IsActive
            };
        }

        public static Patient ToEntity(this PatientDto dto)
        {
            var patient = new Patient
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Gender = dto.Gender,
                CreatedDate = dto.CreatedDate,
                UpdatedDate = dto.UpdatedDate,
                IsActive = dto.IsActive
            };
            
            // Only set ID for updates, not for new entities
            if (dto.Id > 0)
            {
                patient.Id = dto.Id;
            }
            
            return patient;
        }

        // Doctor mappings
        public static DoctorDto ToDto(this Doctor doctor)
        {
            return new DoctorDto
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Specialty = doctor.Specialty,
                Email = doctor.Email,
                Phone = doctor.Phone,
                LicenseNumber = doctor.LicenseNumber,
                CreatedDate = doctor.CreatedDate,
                UpdatedDate = doctor.UpdatedDate,
                IsActive = doctor.IsActive
            };
        }

        public static Doctor ToEntity(this DoctorDto dto)
        {
            var doctor = new Doctor
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Specialty = dto.Specialty,
                Email = dto.Email,
                Phone = dto.Phone,
                LicenseNumber = dto.LicenseNumber,
                CreatedDate = dto.CreatedDate,
                UpdatedDate = dto.UpdatedDate,
                IsActive = dto.IsActive
            };
            
            // Only set ID for updates, not for new entities
            if (dto.Id > 0)
            {
                doctor.Id = dto.Id;
            }
            
            return doctor;
        }

        // Appointment mappings
        public static AppointmentDto ToDto(this Appointment appointment)
        {
            return new AppointmentDto
            {
                Id = appointment.Id,
                PatientId = appointment.PatientId,
                DoctorId = appointment.DoctorId,
                AppointmentDate = appointment.AppointmentDate,
                AppointmentTime = appointment.AppointmentTime,
                Reason = appointment.Reason,
                Notes = appointment.Notes,
                Status = appointment.Status,
                CreatedDate = appointment.CreatedDate,
                UpdatedDate = appointment.UpdatedDate,
                Patient = appointment.Patient != null ? appointment.Patient.ToDto() : null,
                Doctor = appointment.Doctor != null ? appointment.Doctor.ToDto() : null
            };
        }

        public static Appointment ToEntity(this AppointmentDto dto)
        {
            // Only set ID if it's greater than 0 (existing entity)
            // For new entities (Id = 0), let the database generate the ID
            var appointment = new Appointment
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                AppointmentDate = dto.AppointmentDate,
                AppointmentTime = dto.AppointmentTime,
                Reason = dto.Reason,
                Notes = dto.Notes,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Scheduled" : dto.Status,
                CreatedDate = dto.CreatedDate,
                UpdatedDate = dto.UpdatedDate
            };
            
            // Only set ID for updates, not for new entities
            if (dto.Id > 0)
            {
                appointment.Id = dto.Id;
            }
            
            return appointment;
        }

        // Invoice mappings
        public static InvoiceDto ToDto(this Invoice invoice)
        {
            return new InvoiceDto
            {
                Id = invoice.Id,
                AppointmentId = invoice.AppointmentId,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                SubTotal = invoice.SubTotal,
                TaxAmount = invoice.TaxAmount,
                TotalAmount = invoice.TotalAmount,
                Status = invoice.Status,
                PaymentMethod = invoice.PaymentMethod,
                PaidDate = invoice.PaidDate,
                Notes = invoice.Notes,
                CreatedDate = invoice.CreatedDate,
                UpdatedDate = invoice.UpdatedDate
            };
        }

        public static Invoice ToEntity(this InvoiceDto dto)
        {
            return new Invoice
            {
                Id = dto.Id,
                AppointmentId = dto.AppointmentId,
                InvoiceNumber = dto.InvoiceNumber,
                InvoiceDate = dto.InvoiceDate,
                SubTotal = dto.SubTotal,
                TaxAmount = dto.TaxAmount,
                TotalAmount = dto.TotalAmount,
                Status = dto.Status,
                PaymentMethod = dto.PaymentMethod,
                PaidDate = dto.PaidDate,
                Notes = dto.Notes,
                CreatedDate = dto.CreatedDate,
                UpdatedDate = dto.UpdatedDate
            };
        }
    }
}
