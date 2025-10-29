using RXNT.API.Models;

namespace RXNT.API.Repositories
{
    public interface IPatientRepository : IRepository<Patient>
    {
        Task<IEnumerable<Patient>> GetAllPatientsWithAppointmentsAsync();
        Task<Patient?> GetPatientWithAppointmentsAsync(int id);
        Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
    }
}
