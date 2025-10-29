using Microsoft.EntityFrameworkCore;
using RXNT.API.Data;
using RXNT.API.Models;

namespace RXNT.API.Repositories
{
    public class DoctorRepository : Repository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Doctor>> GetAllDoctorsWithAppointmentsAsync()
        {
            return await _dbSet
                .Include(d => d.Appointments)
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();
        }

        public async Task<Doctor?> GetDoctorWithAppointmentsAsync(int id)
        {
            return await _dbSet
                .Include(d => d.Appointments)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
        {
            var query = _dbSet.Where(d => d.Email.ToLower() == email.ToLower());
            
            if (excludeId.HasValue)
            {
                query = query.Where(d => d.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<bool> IsLicenseNumberUniqueAsync(string licenseNumber, int? excludeId = null)
        {
            var query = _dbSet.Where(d => d.LicenseNumber.ToLower() == licenseNumber.ToLower());
            
            if (excludeId.HasValue)
            {
                query = query.Where(d => d.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}
