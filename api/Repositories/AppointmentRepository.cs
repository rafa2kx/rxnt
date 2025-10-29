using Microsoft.EntityFrameworkCore;
using RXNT.API.Data;
using RXNT.API.Models;

namespace RXNT.API.Repositories
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Appointment>> GetAllAppointmentsWithPatientsAsync()
        {
            return await _dbSet
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentWithPatientAsync(int id)
        {
            return await _dbSet
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorAsync(int doctorId)
        {
            return await _dbSet
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
        }
    }
}
