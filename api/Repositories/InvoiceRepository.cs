using Microsoft.EntityFrameworkCore;
using RXNT.API.Data;
using RXNT.API.Models;

namespace RXNT.API.Repositories
{
    public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Invoice>> GetAllInvoicesWithAppointmentsAsync()
        {
            return await _dbSet
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Doctor)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();
        }

        public async Task<Invoice?> GetInvoiceWithAppointmentAsync(int id)
        {
            return await _dbSet
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Patient)
                .Include(i => i.Appointment)
                    .ThenInclude(a => a.Doctor)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            var lastInvoice = await _dbSet
                .OrderByDescending(i => i.CreatedDate)
                .FirstOrDefaultAsync();

            if (lastInvoice == null)
            {
                return $"INV-{DateTime.Now:yyyyMMdd}-0001";
            }

            var parts = lastInvoice.InvoiceNumber.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int lastNumber))
            {
                var newNumber = (lastNumber + 1).ToString("D4");
                return $"INV-{DateTime.Now:yyyyMMdd}-{newNumber}";
            }

            return $"INV-{DateTime.Now:yyyyMMdd}-0001";
        }
    }
}
