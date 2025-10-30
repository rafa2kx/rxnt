using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RXNT.API.Data;
using RXNT.API.DTOs;
using RXNT.API.Models;

namespace RXNT.API.Services
{
    public class BulkUnifiedImportService : IBulkUnifiedImportService
    {
        private readonly ApplicationDbContext _db;
        private readonly IOptions<BulkProcessingOptions> _options;

        public BulkUnifiedImportService(ApplicationDbContext db, IOptions<BulkProcessingOptions> options)
        {
            _db = db;
            _options = options;
        }

        public async Task<(string JobId, string HangfireJobId)> EnqueueProcessingForFile(string filePath)
        {
            var jobId = Guid.NewGuid().ToString("N");

            var record = new BulkJobStatus
            {
                JobId = jobId,
                Status = "Queued",
                SourceFilePath = filePath,
                TotalCount = 0
            };
            _db.BulkJobStatuses.Add(record);
            await _db.SaveChangesAsync();

            var hangfireJobId = BackgroundJob.Enqueue<BulkUnifiedImportProcessor>(p => p.ProcessFile(filePath, jobId));

            record.HangfireJobId = hangfireJobId;
            await _db.SaveChangesAsync();

            return (jobId, hangfireJobId);
        }

        public async Task<BulkStatusResponse?> GetStatusAsync(string jobId)
        {
            var status = await _db.BulkJobStatuses.AsNoTracking().FirstOrDefaultAsync(s => s.JobId == jobId);
            if (status == null) return null;
            return new BulkStatusResponse
            {
                JobId = status.JobId,
                Status = status.Status,
                TotalCount = status.TotalCount,
                ProcessedCount = status.ProcessedCount,
                SuccessCount = status.SuccessCount,
                ErrorCount = status.ErrorCount,
                ErrorSummary = status.ErrorSummary,
                CreatedDate = status.CreatedDate,
                CompletedDate = status.CompletedDate
            };
        }
    }
}


