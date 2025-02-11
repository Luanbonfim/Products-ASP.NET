using System.Diagnostics;

namespace Products.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Log request information
            _logger.LogInformation($"Handling request: {context.Request.Method} {context.Request.Path}");

            // Call the next middleware in the pipeline
            await _next(context);

            // Log response status code and time taken
            stopwatch.Stop();
            _logger.LogInformation($"Handled request: {context.Request.Method} {context.Request.Path} responded {context.Response.StatusCode} in {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
