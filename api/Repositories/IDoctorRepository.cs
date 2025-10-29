using RXNT.API.Models;

namespace RXNT.API.Repositories
{
    public interface IDoctorRepository : IRepository<Doctor>
    {
        Task<IEnumerable<Doctor>> GetAllDoctorsWithAppointmentsAsync();
        Task<Doctor?> GetDoctorWithAppointmentsAsync(int id);
        Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
        Task<bool> IsLicenseNumberUniqueAsync(string licenseNumber, int? excludeId = null);
    }
}
