using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RXNT.API.Data;
using RXNT.API.DTOs;
using RXNT.API.Models;

namespace RXNT.API.Services
{
    public class BulkAppointmentProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICsvAppointmentParser _parser;
        private readonly IOptions<BulkProcessingOptions> _options;

        public BulkAppointmentProcessor(
            IServiceScopeFactory scopeFactory,
            ICsvAppointmentParser parser,
            IOptions<BulkProcessingOptions> options)
        {
            _scopeFactory = scopeFactory;
            _parser = parser;
            _options = options;
        }

        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 120, 300 })]
        public async Task ProcessFile(string filePath, string jobId)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var status = await db.BulkJobStatuses.FirstOrDefaultAsync(s => s.JobId == jobId);
            if (status == null) return;

            status.Status = "Processing";
            await db.SaveChangesAsync();

            var processed = 0;
            var success = 0;
            var errors = 0;

            try
            {
                await using var stream = File.OpenRead(filePath);

                var batch = new List<AppointmentDto>(_options.Value.BatchSize);

                await foreach (var dto in _parser.ParseAsync(stream))
                {
                    batch.Add(dto);
                    if (batch.Count >= _options.Value.BatchSize)
                    {
                        var (ok, err) = await ProcessBatch(scope, batch);
                        processed += batch.Count;
                        success += ok;
                        errors += err;
                        batch.Clear();

                        await UpdateProgress(db, status, processed, success, errors);
                    }
                }

                if (batch.Count > 0)
                {
                    var (ok, err) = await ProcessBatch(scope, batch);
                    processed += batch.Count;
                    success += ok;
                    errors += err;
                    batch.Clear();
                }

                status.Status = errors == 0 ? "Completed" : "Completed"; // Completed regardless; details in counts
                status.ProcessedCount = processed;
                status.SuccessCount = success;
                status.ErrorCount = errors;
                status.CompletedDate = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                status.Status = "Failed";
                status.ErrorSummary = Truncate($"Processing failed: {ex.Message}", 1024);
                status.CompletedDate = DateTime.UtcNow;
                await db.SaveChangesAsync();
                throw;
            }
            finally
            {
                // Cleanup file
                try { if (File.Exists(filePath)) File.Delete(filePath); } catch { /* ignore */ }
            }
        }

        private async Task<(int ok, int err)> ProcessBatch(IServiceScope scope, List<AppointmentDto> batch)
        {
            var booking = scope.ServiceProvider.GetRequiredService<IAppointmentBookingService>();
            var ok = 0;
            var err = 0;
            foreach (var dto in batch)
            {
                try
                {
                    // Delegate invoice creation to booking service (visit fee not in dto; use 0 by default)
                    await booking.ScheduleAppointmentWithInvoiceAsync(
                        dto.PatientId,
                        dto.DoctorId,
                        dto.AppointmentDate,
                        dto.AppointmentTime,
                        dto.Reason,
                        0m);
                    ok++;
                }
                catch
                {
                    err++;
                }
            }
            return (ok, err);
        }

        private static async Task UpdateProgress(ApplicationDbContext db, BulkJobStatus status, int processed, int success, int errors)
        {
            status.ProcessedCount = processed;
            status.SuccessCount = success;
            status.ErrorCount = errors;
            await db.SaveChangesAsync();
        }

        private static string Truncate(string value, int maxChars)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxChars ? value : value.Substring(0, maxChars);
        }
    }
}


