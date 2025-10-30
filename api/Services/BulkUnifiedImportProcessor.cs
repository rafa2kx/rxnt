using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RXNT.API.Data;
using RXNT.API.Models;

namespace RXNT.API.Services
{
    public class BulkUnifiedImportProcessor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICsvUnifiedParser _parser;
        private readonly IOptions<BulkProcessingOptions> _options;

        public BulkUnifiedImportProcessor(
            IServiceScopeFactory scopeFactory,
            ICsvUnifiedParser parser,
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
            var patientService = scope.ServiceProvider.GetRequiredService<IPatientService>();
            var doctorService = scope.ServiceProvider.GetRequiredService<IDoctorService>();
            var bookingService = scope.ServiceProvider.GetRequiredService<IAppointmentBookingService>();

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

                var batch = new List<UnifiedCsvRecord>(_options.Value.BatchSize);

                await foreach (var record in _parser.ParseAsync(stream))
                {
                    batch.Add(record);
                    if (batch.Count >= _options.Value.BatchSize)
                    {
                        var (ok, err) = await ProcessBatch(patientService, doctorService, bookingService, batch);
                        processed += batch.Count;
                        success += ok;
                        errors += err;
                        batch.Clear();
                        await UpdateProgress(db, status, processed, success, errors);
                    }
                }

                if (batch.Count > 0)
                {
                    var (ok, err) = await ProcessBatch(patientService, doctorService, bookingService, batch);
                    processed += batch.Count;
                    success += ok;
                    errors += err;
                    batch.Clear();
                }

                status.Status = errors == 0 ? "Completed" : "Completed";
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
                try { if (File.Exists(filePath)) File.Delete(filePath); } catch { }
            }
        }

        private static async Task<(int ok, int err)> ProcessBatch(
            IPatientService patientService,
            IDoctorService doctorService,
            IAppointmentBookingService bookingService,
            List<UnifiedCsvRecord> batch)
        {
            var ok = 0;
            var err = 0;

            foreach (var r in batch)
            {
                try
                {
                    // Resolve PatientId (use provided or create)
                    int patientId;
                    if (r.PatientId.HasValue && r.PatientId.Value > 0)
                    {
                        patientId = r.PatientId.Value;
                    }
                    else
                    {
                        var createdPatient = await patientService.CreatePatientAsync(r.Patient, true);
                        
                        patientId = createdPatient.Id;
                    }

                    // Resolve DoctorId (use provided or create)
                    int doctorId;
                    if (r.DoctorId.HasValue && r.DoctorId.Value > 0)
                    {
                        doctorId = r.DoctorId.Value;
                    }
                    else
                    {
                        var createdDoctor = await doctorService.CreateDoctorAsync(r.Doctor, true);
                        doctorId = createdDoctor.Id;
                    }

                    // Create appointment with invoice
                    await bookingService.ScheduleAppointmentWithInvoiceAsync(
                        patientId,
                        doctorId,
                        r.Appointment.AppointmentDate,
                        r.Appointment.AppointmentTime,
                        r.Appointment.Reason,
                        r.VisitFee ?? 0m);

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


