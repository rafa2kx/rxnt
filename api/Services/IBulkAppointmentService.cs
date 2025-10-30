using RXNT.API.DTOs;

namespace RXNT.API.Services
{
    public interface IBulkAppointmentService
    {
        Task<(string JobId, string HangfireJobId)> EnqueueProcessingForFile(string filePath);
        Task<BulkStatusResponse?> GetStatusAsync(string jobId);
    }
}


