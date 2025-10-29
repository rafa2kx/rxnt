using Microsoft.EntityFrameworkCore;
using RXNT.API.Data;
using RXNT.API.Models;

namespace RXNT.API.Repositories
{
    public class PatientRepository : Repository<Patient>, IPatientRepository
    {
        public PatientRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Patient>> GetAllPatientsWithAppointmentsAsync()
        {
            return await _dbSet
                .Include(p => p.Appointments)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<Patient?> GetPatientWithAppointmentsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Appointments)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
        {
            var query = _dbSet.Where(p => p.Email.ToLower() == email.ToLower());
            
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}
