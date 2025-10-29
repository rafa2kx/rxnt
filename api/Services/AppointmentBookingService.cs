using RXNT.API.Models;
using RXNT.API.DTOs;
using RXNT.API.Repositories;
using RXNT.API.Extensions;

namespace RXNT.API.Services
{
    public class AppointmentBookingService : IAppointmentBookingService
    {
        private readonly IAppointmentValidationService _appointmentValidationService;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ILogger<AppointmentBookingService> _logger;

        public AppointmentBookingService(
            IAppointmentValidationService appointmentValidationService,
            IAppointmentRepository appointmentRepository,
            IInvoiceRepository invoiceRepository,
            ILogger<AppointmentBookingService> logger)
        {
            _appointmentValidationService = appointmentValidationService;
            _appointmentRepository = appointmentRepository;
            _invoiceRepository = invoiceRepository;
            _logger = logger;
        }

        public async Task<(AppointmentDto Appointment, InvoiceDto Invoice)> ScheduleAppointmentWithInvoiceAsync(
            int patientId, 
            int doctorId, 
            DateTime appointmentDate, 
            string appointmentTime, 
            string reason, 
            decimal visitFee,
            decimal taxRate = 0.08m)
        {
            _logger.LogInformation("Scheduling appointment for Patient {PatientId} with Doctor {DoctorId}", patientId, doctorId);

            // Create appointment
            var appointment = new Appointment
            {
                PatientId = patientId,
                DoctorId = doctorId,
                AppointmentDate = appointmentDate,
                AppointmentTime = appointmentTime,
                Reason = reason,
                Status = "Scheduled",
                CreatedDate = DateTime.UtcNow
            };

            // Validate appointment
            var validation = await _appointmentValidationService.ValidateAppointmentAsync(appointment);
            if (!validation.IsValid)
            {
                _logger.LogWarning("Appointment validation failed: {ErrorMessage}", validation.ErrorMessage);
                throw new InvalidOperationException(validation.ErrorMessage);
            }

            // Calculate invoice amounts
            var taxAmount = visitFee * taxRate;
            var totalAmount = visitFee + taxAmount;

            // Create invoice with navigation property (not foreign key)
            var invoice = new Invoice
            {
                InvoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync(),
                InvoiceDate = DateTime.UtcNow,
                SubTotal = visitFee,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                Status = "Pending",
                CreatedDate = DateTime.UtcNow,
                Appointment = appointment  // Use navigation property instead of AppointmentId
            };

            // Add both appointment and invoice to context
            await _appointmentRepository.AddAsync(appointment);
            await _invoiceRepository.AddAsync(invoice);

            _logger.LogInformation("Appointment and invoice prepared for scheduling");

            return (appointment.ToDto(), invoice.ToDto());
        }
    }
}
