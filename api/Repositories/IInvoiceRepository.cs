using RXNT.API.Models;

namespace RXNT.API.Repositories
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<IEnumerable<Invoice>> GetAllInvoicesWithAppointmentsAsync();
        Task<Invoice?> GetInvoiceWithAppointmentAsync(int id);
        Task<string> GenerateInvoiceNumberAsync();
    }
}
