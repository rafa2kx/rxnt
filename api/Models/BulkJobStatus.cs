using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RXNT.API.Models
{
    public class BulkJobStatus
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string JobId { get; set; } = string.Empty; // External job identifier (returned to client)

        [MaxLength(64)]
        public string? HangfireJobId { get; set; }

        [Required]
        [MaxLength(32)]
        public string Status { get; set; } = "Queued"; // Queued, Processing, Completed, Failed

        public int TotalCount { get; set; }
        public int ProcessedCount { get; set; }
        public int SuccessCount { get; set; }
        public int ErrorCount { get; set; }

        [MaxLength(1024)]
        public string? ErrorSummary { get; set; }

        [MaxLength(512)]
        public string? SourceFilePath { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDate { get; set; }
    }
}


