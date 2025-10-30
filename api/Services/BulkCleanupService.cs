using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RXNT.API.Data;

namespace RXNT.API.Services
{
    public class BulkCleanupService
    {
        private readonly ApplicationDbContext _db;
        private readonly IOptions<BulkProcessingOptions> _options;

        public BulkCleanupService(ApplicationDbContext db, IOptions<BulkProcessingOptions> options)
        {
            _db = db;
            _options = options;
        }

        // Run daily to delete orphan temp files and trim old completed statuses (> 14 days)
        [AutomaticRetry(Attempts = 1)]
        public async Task CleanupAsync()
        {
            // Delete files that no longer have jobs, or older than 2 days
            try
            {
                var dir = _options.Value.UploadPath;
                if (Directory.Exists(dir))
                {
                    var threshold = DateTime.UtcNow.AddDays(-2);
                    foreach (var file in Directory.EnumerateFiles(dir))
                    {
                        try
                        {
                            var info = new FileInfo(file);
                            if (info.CreationTimeUtc < threshold)
                            {
                                File.Delete(file);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            // Remove old completed/failed statuses
            var cutoff = DateTime.UtcNow.AddDays(-14);
            var old = await _db.BulkJobStatuses
                .Where(s => (s.Status == "Completed" || s.Status == "Failed") && s.CompletedDate != null && s.CompletedDate < cutoff)
                .ToListAsync();

            if (old.Count > 0)
            {
                _db.BulkJobStatuses.RemoveRange(old);
                await _db.SaveChangesAsync();
            }
        }
    }
}


