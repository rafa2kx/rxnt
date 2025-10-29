using RXNT.API.Models;

namespace RXNT.API.Repositories
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<IEnumerable<Appointment>> GetAllAppointmentsWithPatientsAsync();
        Task<Appointment?> GetAppointmentWithPatientAsync(int id);
        Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId);
    }
}
