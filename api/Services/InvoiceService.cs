using RXNT.API.Data;
using RXNT.API.Models;
using RXNT.API.DTOs;
using RXNT.API.Repositories;
using RXNT.API.Extensions;
using Microsoft.EntityFrameworkCore;

namespace RXNT.API.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            IAppointmentRepository appointmentRepository,
            ApplicationDbContext context,
            ILogger<InvoiceService> logger)
        {
            _invoiceRepository = invoiceRepository;
            _appointmentRepository = appointmentRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync()
        {
            try
            {
                _logger.LogInformation("Getting all invoices");
                var invoices = await _invoiceRepository.GetAllInvoicesWithAppointmentsAsync();
                return invoices.Select(i => i.ToDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all invoices");
                throw;
            }
        }

        public async Task<InvoiceDto?> GetInvoiceByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting invoice with ID: {InvoiceId}", id);
                var invoice = await _invoiceRepository.GetInvoiceWithAppointmentAsync(id);
                return invoice?.ToDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice with ID: {InvoiceId}", id);
                throw;
            }
        }

        public async Task<InvoiceDto> CreateInvoiceAsync(InvoiceDto invoiceDto)
        {
            _logger.LogInformation("Creating invoice for appointment ID: {AppointmentId}", invoiceDto.AppointmentId);

            var invoice = invoiceDto.ToEntity();

            // Validate appointment exists
            var appointment = await _appointmentRepository.GetByIdAsync(invoice.AppointmentId);
            if (appointment == null)
            {
                throw new InvalidOperationException("Appointment not found.");
            }

            // Check if invoice already exists for this appointment
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.AppointmentId == invoice.AppointmentId);
            
            if (existingInvoice != null)
            {
                throw new InvalidOperationException("An invoice already exists for this appointment.");
            }

            // Business logic: Set default values
            invoice.InvoiceNumber = await _invoiceRepository.GenerateInvoiceNumberAsync();
            invoice.InvoiceDate = DateTime.UtcNow;
            invoice.Status = "Pending";
            invoice.CreatedDate = DateTime.UtcNow;

            // Call repository for data access
            await _invoiceRepository.AddAsync(invoice);

            _logger.LogInformation("Invoice created successfully with number: {InvoiceNumber}", invoice.InvoiceNumber);
            return invoice.ToDto();
        }

        public async Task<InvoiceDto?> UpdateInvoiceAsync(int id, InvoiceDto invoiceDto)
        {
            _logger.LogInformation("Updating invoice with ID: {InvoiceId}", id);

            var invoice = invoiceDto.ToEntity();

            var existingInvoice = await _invoiceRepository.GetByIdAsync(id);
            if (existingInvoice == null)
            {
                _logger.LogWarning("Invoice not found with ID: {InvoiceId}", id);
                return null;
            }

            // Business logic: Update properties (limited updates allowed)
            existingInvoice.Notes = invoice.Notes;
            existingInvoice.SubTotal = invoice.SubTotal;
            existingInvoice.TaxAmount = invoice.TaxAmount;
            existingInvoice.TotalAmount = invoice.TotalAmount;
            existingInvoice.Status = invoice.Status;
            existingInvoice.UpdatedDate = DateTime.UtcNow;

            // Call repository for data access
            await _invoiceRepository.UpdateAsync(existingInvoice);

            _logger.LogInformation("Invoice updated successfully");
            return existingInvoice.ToDto();
        }

        public async Task<bool> DeleteInvoiceAsync(int id)
        {
            _logger.LogInformation("Deleting invoice with ID: {InvoiceId}", id);

            var invoice = await _invoiceRepository.GetByIdAsync(id);
            if (invoice == null)
            {
                _logger.LogWarning("Invoice not found with ID: {InvoiceId}", id);
                return false;
            }

            // Call repository for data access
            await _invoiceRepository.DeleteAsync(invoice);

            _logger.LogInformation("Invoice deleted successfully");
            return true;
        }

        public async Task<bool> MarkInvoiceAsPaidAsync(int id, string paymentMethod)
        {
            _logger.LogInformation("Marking invoice {InvoiceId} as paid with payment method: {PaymentMethod}", id, paymentMethod);

            var invoice = await _invoiceRepository.GetByIdAsync(id);
            if (invoice == null)
            {
                _logger.LogWarning("Invoice not found with ID: {InvoiceId}", id);
                return false;
            }

            // Business logic: Mark as paid
            invoice.Status = "Paid";
            invoice.PaymentMethod = paymentMethod;
            invoice.PaidDate = DateTime.UtcNow;
            invoice.UpdatedDate = DateTime.UtcNow;

            // Call repository for data access
            await _invoiceRepository.UpdateAsync(invoice);

            _logger.LogInformation("Invoice marked as paid successfully");
            return true;
        }

        public async Task<(bool IsValid, string ErrorMessage)> ValidateInvoiceAsync(Invoice invoice)
        {
            // Basic validation
            if (invoice.AppointmentId <= 0)
            {
                return (false, "Valid appointment ID is required.");
            }

            if (invoice.SubTotal < 0)
            {
                return (false, "Subtotal cannot be negative.");
            }

            if (invoice.TotalAmount < 0)
            {
                return (false, "Total amount cannot be negative.");
            }

            return (true, string.Empty);
        }
    }
}
