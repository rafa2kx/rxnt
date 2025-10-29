using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RXNT.API.Data;

namespace RXNT.API.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected readonly ApplicationDbContext _context;
        protected readonly ILogger<BaseController> _logger;

        protected BaseController(ApplicationDbContext context, ILogger<BaseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Executes an action within a database transaction
        /// </summary>
        protected async Task<IActionResult> ExecuteWithTransactionAsync<T>(Func<Task<T>> action)
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await action();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Transaction completed successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Transaction rolled back due to error");
                return HandleException(ex, "Operation");
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        /// <summary>
        /// Executes an action within a database transaction (for void operations)
        /// </summary>
        protected async Task<IActionResult> ExecuteWithTransactionAsync(Func<Task> action)
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await action();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Transaction completed successfully");
                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Transaction rolled back due to error");
                return HandleException(ex, "Operation");
            }
            finally
            {
                await transaction.DisposeAsync();
            }
        }

        /// <summary>
        /// Handles exceptions and returns appropriate HTTP responses
        /// </summary>
        protected IActionResult HandleException(Exception ex, string operation)
        {
            _logger.LogError(ex, "Error in {Operation}", operation);

            return ex switch
            {
                InvalidOperationException => BadRequest(new { message = ex.Message }),
                KeyNotFoundException => NotFound(new { message = ex.Message }),
                DbUpdateException => StatusCode(500, new { message = "A database error occurred. Please try again." }),
                _ => StatusCode(500, new { message = "An unexpected error occurred." })
            };
        }
    }
}
