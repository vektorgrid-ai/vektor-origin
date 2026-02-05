using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AssistantCore.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                          ?? context.TraceIdentifier
                          ?? Activity.Current?.TraceId.ToString();

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            _logger.LogDebug("HTTP {Method} {Path} starting", context.Request.Method, context.Request.Path);

            try
            {
                await _next(context);
                sw.Stop();
                _logger.LogDebug("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    sw.Elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Unhandled exception processing {Method} {Path} after {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    sw.Elapsed.TotalMilliseconds);
                throw;
            }
        }
    }
}

