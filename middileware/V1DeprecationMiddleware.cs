namespace TmsApi.Middleware;

public class V1DeprecationMiddleware(RequestDelegate next)
{
  // V1 sunset date — Dec 31, 2026
  private static readonly DateTimeOffset SunsetDate =
      new(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);

  public async Task InvokeAsync(HttpContext context)
  {
    // OnStarting = headers flushed but before body — correct timing
    context.Response.OnStarting(() =>
    {
      if (context.Request.Path.StartsWithSegments("/api/v1"))
      {
        context.Response.Headers["Deprecation"] = "true";
        context.Response.Headers["Sunset"] =
                SunsetDate.ToString("R");
        context.Response.Headers["Link"] =
                $"<{context.Request.Scheme}://{context.Request.Host}" +
                $"/api/v2{context.Request.Path.Value?[7..]}>" +
                "; rel=\"successor-version\"";
      }
      return Task.CompletedTask;
    });

    await next(context);
  }
}