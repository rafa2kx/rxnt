using System.Globalization;
using RXNT.API.DTOs;

namespace RXNT.API.Services
{
    public interface ICsvAppointmentParser
    {
        IAsyncEnumerable<AppointmentDto> ParseAsync(Stream csvStream, CancellationToken cancellationToken = default);
        int? CountHeaderColumns(string headerLine);
    }

    public class CsvAppointmentParser : ICsvAppointmentParser
    {
        private static readonly string[] ExpectedHeaders = new[]
        {
            "PatientId","DoctorId","AppointmentDate","AppointmentTime","Reason","VisitFee"
        };

        public int? CountHeaderColumns(string headerLine)
        {
            if (string.IsNullOrWhiteSpace(headerLine)) return null;
            return headerLine.Split(',').Length;
        }

        public async IAsyncEnumerable<AppointmentDto> ParseAsync(Stream csvStream, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var reader = new StreamReader(csvStream);
            string? line;

            // Read header
            var header = await reader.ReadLineAsync();
            if (header is null)
                yield break;

            // Basic header validation: contains expected columns in order
            var columns = header.Split(',');
            if (columns.Length < ExpectedHeaders.Length)
                yield break;

            // Parse lines
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = SplitCsvLine(line);
                if (parts.Length < ExpectedHeaders.Length)
                    continue;

                if (!int.TryParse(parts[0], out var patientId)) continue;
                if (!int.TryParse(parts[1], out var doctorId)) continue;
                if (!DateTime.TryParse(parts[2], CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var appointmentDate)) continue;
                var appointmentTime = parts[3];
                var reason = parts[4];

                // VisitFee present but not used in AppointmentDto; upstream service will handle invoice
                _ = decimal.TryParse(parts[5], NumberStyles.Number, CultureInfo.InvariantCulture, out var _);

                yield return new AppointmentDto
                {
                    PatientId = patientId,
                    DoctorId = doctorId,
                    AppointmentDate = appointmentDate,
                    AppointmentTime = appointmentTime,
                    Reason = reason,
                    Notes = string.Empty,
                    Status = "Scheduled",
                    CreatedDate = DateTime.UtcNow
                };
            }
        }

        private static string[] SplitCsvLine(string line)
        {
            // Simple CSV split (no quoted field handling). Replace with robust parser if needed.
            return line.Split(',');
        }
    }
}


