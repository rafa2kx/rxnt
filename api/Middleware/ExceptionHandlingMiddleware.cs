using System.Net;
using System.Text.Json;

namespace RXNT.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = context.Response;

            var errorResponse = new ErrorResponse
            {
                Message = exception.Message,
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Timestamp = DateTime.UtcNow
            };

            switch (exception)
            {
                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                    _logger.LogWarning(exception, "Bad request: {Message}", exception.Message);
                    break;

                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                    _logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    errorResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                    _logger.LogWarning(exception, "Unauthorized access: {Message}", exception.Message);
                    break;

                default:
                    _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                    errorResponse.Message = "An error occurred while processing your request.";
                    break;
            }

            errorResponse.StatusCode = response.StatusCode;
            var jsonResponse = JsonSerializer.Serialize(errorResponse);

            await response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
