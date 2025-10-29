using Microsoft.AspNetCore.Mvc;
using RXNT.API.Data;
using RXNT.API.Models;
using RXNT.API.DTOs;
using RXNT.API.Services;

namespace RXNT.API.Controllers
{
    [Route("api/[controller]")]
    public class AppointmentsController : BaseController
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAppointmentBookingService _bookingService;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IAppointmentBookingService bookingService,
            ApplicationDbContext context,
            ILogger<BaseController> logger)
            : base(context, logger)
        {
            _appointmentService = appointmentService;
            _bookingService = bookingService;
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
