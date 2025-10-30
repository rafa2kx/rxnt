using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RXNT.API.Data;
using RXNT.API.DTOs;
using RXNT.API.Services;

namespace RXNT.API.Controllers
{
    [Route("api/[controller]")]
    public class ImportsController : BaseController
    {
        private readonly IBulkUnifiedImportService _bulkService;
        private readonly IOptions<BulkProcessingOptions> _options;

        public ImportsController(
            IBulkUnifiedImportService bulkService,
            IOptions<BulkProcessingOptions> options,
            ApplicationDbContext context,
            ILogger<BaseController> logger) : base(context, logger)
        {
            _bulkService = bulkService;
            _options = options;
        }

        [HttpPost("bulk-upload")] 
        [RequestSizeLimit(104_857_600)]
        [Consumes("application/octet-stream", "text/csv")]
        public async Task<IActionResult> UploadBulkAppointments([FromQuery] string? filename = null)
        {
            // Allow client to provide filename via header if not supplied as query
            if (string.IsNullOrWhiteSpace(filename) && Request.Headers.TryGetValue("X-File-Name", out var headerVals))
            {
                filename = headerVals.FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(filename))
                return BadRequest("Missing filename. Provide as query parameter 'filename' or header 'X-File-Name'.");

            var ext = Path.GetExtension(filename).ToLowerInvariant();
            if (ext != ".csv")
                return BadRequest("Only .csv files are supported");

            var maxBytes = _options.Value.MaxFileSizeMb * 1024L * 1024L;
            if (Request.ContentLength.HasValue && Request.ContentLength.Value > maxBytes)
                return BadRequest($"File exceeds max size of {_options.Value.MaxFileSizeMb} MB");

            Directory.CreateDirectory(_options.Value.UploadPath);
            var name = $"{Guid.NewGuid():N}_{filename}";
            var path = Path.Combine(_options.Value.UploadPath, name);
            // Stream request body directly to file to avoid buffering entire file in memory
            await using (var targetStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await Request.Body.CopyToAsync(targetStream);
            }

            var (jobId, hangfireJobId) = await _bulkService.EnqueueProcessingForFile(path);
            return Accepted(new BulkUploadResponse { JobId = jobId, HangfireJobId = hangfireJobId, FileName = name, Status = "Queued" });
        }

        [HttpGet("bulk-status/{jobId}")]
        public async Task<IActionResult> Status(string jobId)
        {
            var status = await _bulkService.GetStatusAsync(jobId);
            if (status == null) return NotFound();
            return Ok(status);
        }
    }
}


