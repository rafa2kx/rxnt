using Microsoft.AspNetCore.Mvc;
using RXNT.API.Data;
using RXNT.API.Models;
using RXNT.API.DTOs;
using RXNT.API.Services;
using Microsoft.Extensions.Options;
using RXNT.API.Models.Requests;

namespace RXNT.API.Controllers
{
    [Route("api/[controller]")]
    public class AppointmentsController : BaseController
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAppointmentBookingService _bookingService;
        private readonly IBulkAppointmentService _bulkService;
        private readonly IOptions<BulkProcessingOptions> _bulkOptions;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IAppointmentBookingService bookingService,
            IBulkAppointmentService bulkService,
            IOptions<BulkProcessingOptions> bulkOptions,
            ApplicationDbContext context,
            ILogger<BaseController> logger)
            : base(context, logger)
        {
            _appointmentService = appointmentService;
            _bookingService = bookingService;
            _bulkService = bulkService;
            _bulkOptions = bulkOptions;
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync();
            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return NotFound();

            return Ok(appointment);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment(AppointmentDto appointment)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var createdAppointment = await _appointmentService.CreateAppointmentAsync(appointment);
                return CreatedAtAction(nameof(GetAppointment), new { id = createdAppointment.Id }, createdAppointment);
            });
        }

        [HttpPost("bulk-status")]
        public async Task<IActionResult> BulkUpdateStatus(BulkAppointmentStatusUpdateRequest request)
        {
            //return await ExecuteWithTransactionAsync(async () =>
            //{
            var updated = await _appointmentService.BulkUpdateStatusAsync(request.Status, request.Ids);
            return Ok("Success");
            //});
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, AppointmentDto appointment)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var updatedAppointment = await _appointmentService.UpdateAppointmentAsync(id, appointment);
                if (updatedAppointment == null)
                    throw new KeyNotFoundException($"Appointment with ID {id} not found");

                return updatedAppointment;
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var result = await _appointmentService.DeleteAppointmentAsync(id);
                if (!result)
                    throw new KeyNotFoundException($"Appointment with ID {id} not found");
            });
        }

        [HttpPost("schedule-with-invoice")]
        public async Task<IActionResult> ScheduleAppointmentWithInvoice([FromBody] ScheduleAppointmentRequest request)
        {
            return await ExecuteWithTransactionAsync(async () =>
            {
                var result = await _bookingService.ScheduleAppointmentWithInvoiceAsync(
                    request.PatientId,
                    request.DoctorId,
                    request.AppointmentDate,
                    request.AppointmentTime,
                    request.Reason,
                    request.VisitFee,
                    request.TaxRate);

                return CreatedAtAction(
                    nameof(GetAppointment), 
                    new { id = result.Appointment.Id }, 
                    new { Appointment = result.Appointment, Invoice = result.Invoice });
            });
        }

        // Accept raw binary (application/octet-stream or text/csv).
        // Filename must be supplied either as query parameter `filename` or header `X-File-Name`.
        [HttpPost("bulk-upload")]
        [RequestSizeLimit(104_857_600)] // ~100MB
        [Consumes("application/octet-stream", "text/csv")]
        public async Task<IActionResult> UploadBulkAppointments([FromQuery] string? filename = null)
        {
            // Allow client to provide filename via header if not supplied as query
            if (string.IsNullOrWhiteSpace(filename) && Request.Headers.TryGetValue("X-File-Name", out var headerVals))
            {
                filename = headerVals.FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(filename))
                return BadRequest("Missing filename. Provide as query parameter 'filename' or header 'X-File-Name'.");

            var ext = Path.GetExtension(filename).ToLowerInvariant();
            if (ext != ".csv")
                return BadRequest("Only .csv files are supported");

            var maxBytes = _bulkOptions.Value.MaxFileSizeMb * 1024L * 1024L;
            if (Request.ContentLength.HasValue && Request.ContentLength.Value > maxBytes)
                return BadRequest($"File exceeds max size of {_bulkOptions.Value.MaxFileSizeMb} MB");

            Directory.CreateDirectory(_bulkOptions.Value.UploadPath);
            var unique = Guid.NewGuid().ToString("N");
            var safeFileName = Path.GetFileName(filename);
            var fileName = $"{unique}_{safeFileName}";
            var filePath = Path.Combine(_bulkOptions.Value.UploadPath, fileName);

            // Stream request body directly to file to avoid buffering entire file in memory
            await using (var targetStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await Request.Body.CopyToAsync(targetStream);
            }

            // Optionally, you could validate the CSV contents here before enqueuing

            var (jobId, hangfireJobId) = await _bulkService.EnqueueProcessingForFile(filePath);

            return Accepted(new RXNT.API.DTOs.BulkUploadResponse
            {
                JobId = jobId,
                HangfireJobId = hangfireJobId,
                FileName = fileName,
                Status = "Queued"
            });
        }

        [HttpGet("bulk-status/{jobId}")]
        public async Task<IActionResult> GetBulkStatus(string jobId)
        {
            var status = await _bulkService.GetStatusAsync(jobId);
            if (status == null) return NotFound();
            return Ok(status);
        }
    }

    // Request model
    public class ScheduleAppointmentRequest
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public decimal VisitFee { get; set; }
        public decimal TaxRate { get; set; } = 0.08m;
    }
}
