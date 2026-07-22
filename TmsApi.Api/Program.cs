
//  Module 7 Ex4

using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Api.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using TmsApi.Api.ExceptionHandlers;
using TmsApi.Api.middileware;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Commands;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Data;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using Microsoft.Extensions.Caching.Hybrid;
using TmsApi.Infrastructure.Caching;
using Microsoft.Extensions.Options;
// using TmsApi.Application.Interfaces;
// using Microsoft.Extensions.Caching.Hybrid;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// Register Services
// -------------------------

builder.Services.AddProblemDetails();

builder.Services.AddControllers();

builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase")));

builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<EnrollmentReportService>();
builder.Services.AddScoped<StudentUpdateService>();
builder.Services.AddScoped<ArchiveService>();

// -------------------------
// API Versioning
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";  // ← lowercase v
        options.SubstituteApiVersionInUrl = true;
    });

// OpenAPI Documents
builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v1";  // ← lowercase v
});

builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v2";  // ← lowercase v
});


builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EnrollStudentHandler).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(EnrollStudentValidator).Assembly);
// LoggingBehavior FIRST—it must wrap ValidationBehavior 
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
    };
});
// 
builder.Services.AddRateLimiter(options =>
{
    // GlobalLimiter — every request goes through this
    options.GlobalLimiter = PartitionedRateLimiter
        .Create<HttpContext, string>(httpContext =>
        {
            var (partitionKey, tier) =
                ApiKeyResolver.Resolve(httpContext);

            return tier switch
            {
                // Paid: 200 tokens, +100 per 10s
                ApiKeyTier.Paid =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: $"paid:{partitionKey}",
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 200,
                            TokensPerPeriod = 100,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }),

                // Free: 30 tokens, +10 per 10s
                ApiKeyTier.Free =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: $"free:{partitionKey}",
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 30,
                            TokensPerPeriod = 10,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }),

                // Anonymous: 10 tokens, +5 per 10s
                _ =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: $"anon:{partitionKey}",
                        factory: _ => new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 10,
                            TokensPerPeriod = 5,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        })
            };
        });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // OnRejected — ProblemDetails + Retry-After header
    options.OnRejected = async (context, ct) =>
    {
        // Retry-After from lease metadata — not hard-coded!
        var retryAfter = "10";
        if (context.Lease.TryGetMetadata(
                MetadataName.RetryAfter, out var ts))
            retryAfter = ((int)ts.TotalSeconds).ToString();

        context.HttpContext.Response.Headers.RetryAfter = retryAfter;
        context.HttpContext.Response.ContentType =
            "application/problem+json";

        await context.HttpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Title = "Rate limit exceeded",
                Detail = $"Too many requests. Retry after {retryAfter} seconds.",
                Status = StatusCodes.Status429TooManyRequests,
                Type = "https://tms.local/errors/rate_limit_exceeded"
            }, ct);
    };

    // Concurrency limiter for transcript endpoint
    options.AddConcurrencyLimiter("transcripts", opt =>
    {
        opt.PermitLimit = 5;   // 5 in-flight max
        opt.QueueLimit = 20;  // queue 20 more
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // Named search policy — tighter cap
    options.AddTokenBucketLimiter("search", opt =>
    {
        opt.TokenLimit = 10;
        opt.TokensPerPeriod = 5;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        opt.QueueLimit = 2;
    });

    // Token Bucket — per-second throttle
    // 10 req / 10s → ok
    // 11th req   → 429 (tokens empty)

    // Concurrency Limiter — simultaneous limit
    // 5 transcripts running now → 6th queues (up to 20)
    // 26th → 429 (queue full)

    // Both apply to /api/v2/transcripts:
    // 1. GlobalLimiter (token bucket) checks first
    // 2. "transcripts" (concurrency) checks second
    // Both must pass → request proceeds

});
builder.Services.AddHealthChecks();

// App
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/openapi/v1.json");
    app.MapOpenApi("/openapi/v2.json");

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("TMS API Explorer")
            .WithTheme(ScalarTheme.DeepSpace)
            .AddDocument("v1", "/openapi/v1.json")
            .AddDocument("v2", "/openapi/v2.json");
    });
}



app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseMiddleware<V1DeprecationMiddleware>();

// Seed — Development only!
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
}
app.UseRateLimiter();
app.MapHealthChecks("/health/live").DisableRateLimiting();
app.MapHealthChecks("/health/ready").DisableRateLimiting();

app.MapControllers();
app.Run();