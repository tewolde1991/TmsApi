// using System.Diagnostics;

// public class RequestLoggingMiddleware
// {
//     private readonly RequestDelegate _next;
//     private readonly ILogger<RequestLoggingMiddleware> _logger;

//     public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
//     {
//         _next = next;
//         _logger = logger;
//     }

//     public async Task InvokeAsync(HttpContext context)
//     {
//         // generate a short corellation id
//         string correlationId = Guid.NewGuid().ToString("N")[..8];
//         // Set context response header must be before next
//         context.Response.Headers["x-Correlation-Id"] = correlationId;

//         // log the request start
//         _logger.LogInformation(
//             "Request started: {Method} {Path} [Correlation: {CorrelationId}]",
//             context.Request.Method,
//             context.Request.Path,
//             correlationId
//         );
//         // start timing
//         var sw = Stopwatch.StartNew();

//         // call the next middlware
//         await _next(context);
//         // stop timing
//         sw.Stop();
//     // 7. Log the request completion with status code and elapsed time
//     _logger.LogInformation(
//       "Request finished: {Method},{Path} respond {StatusCode} in {Eleapsed} ms [Correlation: {CorrelationId}]",
//         context.Request.Method,
//         context.Request.Path,
//         context.Response.StatusCode,
//         sw.ElapsedMilliseconds,
//         correlationId
//     );
//     }

// }

// namespace TmsApi;
using System.Diagnostics;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync( HttpContext context)
    {
        string correlationId = Guid.NewGuid().ToString("N")[..8];
        // SET context response
        context.Response.Headers["X-Correlation-id"] = correlationId;

        // LOG THE REQUEST
        _logger.LogInformation(
            "Request started: {method} {path} [Correlation:{correkationId}]",
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
    // log the request completion witsh status code elpsd ms and id
    _logger.LogInformation(
        "Request finished {method} {path}Respond {statusCode}in {elepsed} [correlation:{correlationId}]",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        sw.ElapsedMilliseconds,
        correlationId
    );
    }

}