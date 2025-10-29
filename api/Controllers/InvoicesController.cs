using Microsoft.AspNetCore.Mvc;
using RXNT.API.Data;
using RXNT.API.Models;
using RXNT.API.DTOs;
using RXNT.API.Services;

namespace RXNT.API.Controllers
{
    [Route("api/[controller]")]
    public class InvoicesController : BaseController
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(
            IInvoiceService invoiceService,
            ApplicationDbContext context,
            ILogger<BaseController> logger)
            : base(context, logger)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoices()
        {
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            return Ok(invoices);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null)
                return NotFound();

            return Ok(invoice);
        }

        [HttpPost]
        public async Task<IActionResult> CreateInvoice(InvoiceDto invoice)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var createdInvoice = await _invoiceService.CreateInvoiceAsync(invoice);
                return CreatedAtAction(nameof(GetInvoice), new { id = createdInvoice.Id }, createdInvoice);
            });
        }

        [HttpPut("{id}/pay")]
        public async Task<IActionResult> MarkAsPaid(int id, [FromBody] PaymentRequest request)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var result = await _invoiceService.MarkInvoiceAsPaidAsync(id, request.PaymentMethod);
                if (!result)
                    throw new KeyNotFoundException($"Invoice with ID {id} not found");
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, InvoiceDto invoice)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var updatedInvoice = await _invoiceService.UpdateInvoiceAsync(id, invoice);
                if (updatedInvoice == null)
                    throw new KeyNotFoundException($"Invoice with ID {id} not found");

                return updatedInvoice;
            });
        }
    }

    // Request models
    public class CreateInvoiceRequest
    {
        public int AppointmentId { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxRate { get; set; } = 0.08m;
    }

    public class PaymentRequest
    {
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
