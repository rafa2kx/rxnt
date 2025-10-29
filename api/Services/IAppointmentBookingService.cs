using RXNT.API.Models;
using RXNT.API.DTOs;

namespace RXNT.API.Services
{
    public interface IAppointmentBookingService
    {
        Task<(AppointmentDto Appointment, InvoiceDto Invoice)> ScheduleAppointmentWithInvoiceAsync(
            int patientId, 
            int doctorId, 
            DateTime appointmentDate, 
            string appointmentTime, 
            string reason, 
            decimal visitFee,
            decimal taxRate = 0.08m);
    }
}
