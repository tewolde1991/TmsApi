using System.Diagnostics;

namespace TmsApi.Api.middileware;

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
        string correlationId = Guid.NewGuid().ToString("N")[..8];
        // SET context response
        context.Response.Headers["X-Correlation-id"] = correlationId;

        // LOG THE REQUEST
        _logger.LogInformation(
            "Request started: {method} {path} [Correlation:{correlationId}]",
            context.Request.Method,
            context.Request.Path,
            correlationId

        );
        // start timing
        var sw = Stopwatch.StartNew();

        // call the next middleware
        await _next(context);
        // stop timing
        sw.Stop();
        // log the request completion with status code elapsed ms and id
        _logger.LogInformation(
            "Request finished {method} {path}Respond {statusCode}in {elapsed} [correlation:{correlationId}]",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds,
            correlationId
        );
    }

}