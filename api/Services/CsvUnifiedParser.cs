using System.Globalization;
using RXNT.API.DTOs;

namespace RXNT.API.Services
{
    public class UnifiedCsvRecord
    {
        public int? PatientId { get; set; }
        public PatientDto Patient { get; set; } = new PatientDto();
        public int? DoctorId { get; set; }
        public DoctorDto Doctor { get; set; } = new DoctorDto();
        public AppointmentDto Appointment { get; set; } = new AppointmentDto();
        public decimal? VisitFee { get; set; }
    }

    public interface ICsvUnifiedParser
    {
        IAsyncEnumerable<UnifiedCsvRecord> ParseAsync(Stream csvStream, CancellationToken cancellationToken = default);
    }

    public class CsvUnifiedParser : ICsvUnifiedParser
    {
        // Row schema: patient + doctor + appointment fields in the same line.
        // Optional: PatientId, DoctorId for existing records.
        // Header (order-insensitive):
        // PatientId,PatientFirstName,PatientLastName,PatientEmail,PatientPhone,PatientDob,
        // DoctorId,DoctorFirstName,DoctorLastName,DoctorEmail,DoctorPhone,Specialty,LicenseNumber,
        // AppointmentDate,AppointmentTime,Reason,VisitFee
        public async IAsyncEnumerable<UnifiedCsvRecord> ParseAsync(Stream csvStream, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var reader = new StreamReader(csvStream);
            var header = await reader.ReadLineAsync();
            if (header is null) yield break;

            var headers = header.Split(',');
            var map = headers
                .Select((h, i) => new { h = h.Trim(), i })
                .ToDictionary(x => x.h, x => x.i, StringComparer.OrdinalIgnoreCase);

            int ReadIndex(string name) => map.TryGetValue(name, out var idx) ? idx : -1;

            // Patient indexes
            var idxPId = ReadIndex("PatientId");
            var idxPFn = ReadIndex("PatientFirstName");
            var idxPLn = ReadIndex("PatientLastName");
            var idxPEmail = ReadIndex("PatientEmail");
            var idxPPhone = ReadIndex("PatientPhone");
            var idxPDob = ReadIndex("PatientDob");

            // Doctor indexes
            var idxDId = ReadIndex("DoctorId");
            var idxDFn = ReadIndex("DoctorFirstName");
            var idxDLn = ReadIndex("DoctorLastName");
            var idxDEmail = ReadIndex("DoctorEmail");
            var idxDPhone = ReadIndex("DoctorPhone");
            var idxDSpec = ReadIndex("Specialty");
            var idxDLicense = ReadIndex("LicenseNumber");

            // Appointment indexes
            var idxADate = ReadIndex("AppointmentDate");
            var idxATime = ReadIndex("AppointmentTime");
            var idxAReason = ReadIndex("Reason");
            var idxAVisitFee = ReadIndex("VisitFee");

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (cancellationToken.IsCancellationRequested) yield break;
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cells = line.Split(',');
                string Get(int idx) => idx >= 0 && idx < cells.Length ? cells[idx].Trim() : string.Empty;

                int? TryParseInt(string s) => int.TryParse(s, out var v) ? v : (int?)null;
                DateTime? TryParseDate(string s) => DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var v) ? v : (DateTime?)null;
                decimal? TryParseDecimal(string s) => decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : (decimal?)null;

                yield return new UnifiedCsvRecord
                {
                    PatientId = TryParseInt(Get(idxPId)),
                    Patient = new PatientDto
                    {
                        FirstName = Get(idxPFn),
                        LastName = Get(idxPLn),
                        Email = Get(idxPEmail),
                        Phone = Get(idxPPhone),
                        DateOfBirth = TryParseDate(Get(idxPDob)).GetValueOrDefault()
                    },
                    DoctorId = TryParseInt(Get(idxDId)),
                    Doctor = new DoctorDto
                    {
                        FirstName = Get(idxDFn),
                        LastName = Get(idxDLn),
                        Email = Get(idxDEmail),
                        Phone = Get(idxDPhone),
                        Specialty = Get(idxDSpec),
                        LicenseNumber = Get(idxDLicense)
                    },
                    Appointment = new AppointmentDto
                    {
                        AppointmentDate = TryParseDate(Get(idxADate)) ?? DateTime.UtcNow,
                        AppointmentTime = Get(idxATime),
                        Reason = Get(idxAReason),
                        Notes = string.Empty,
                        Status = "Scheduled",
                        CreatedDate = DateTime.UtcNow
                    },
                    VisitFee = TryParseDecimal(Get(idxAVisitFee))
                };
            }
        }
    }
}


