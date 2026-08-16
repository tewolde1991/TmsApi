// Module 7 Ex5 + Ex6 — Session 3

using System.Threading.Channels;
using System.Threading.RateLimiting;
using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Scalar.AspNetCore;
using TmsApi.Api.ExceptionHandlers;
using TmsApi.Api.Hubs;
using TmsApi.Api.middileware;
using TmsApi.Api.RateLimiting;
using TmsApi.Application.Behaviors;
using TmsApi.Application.Commands;
using TmsApi.Application.Hubs;

// using TmsApi.Application.Hubs;
using TmsApi.Application.Interfaces;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Data;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Infrastructure.Transcripts;
using TmsApi.Infrastructure.Workers;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200"];

// -------------------------
// Core Services
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
// -------------------------
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        // Read version from URL segment, query string, OR header
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new QueryStringApiVersionReader("api-version"),
            new HeaderApiVersionReader("api-version")
        );
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

// -------------------------
// OpenAPI / Scalar
// -------------------------
builder.Services.AddOpenApi("v1", options =>
{
    options.ShouldInclude = description => description.GroupName == "v1";
});
builder.Services.AddOpenApi("v2", options =>
{
    options.ShouldInclude = description => description.GroupName == "v2";
});

// -------------------------
// MediatR + Validation
// -------------------------
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(EnrollStudentHandler).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(EnrollStudentValidator).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// -------------------------
// Repositories + Cache
// -------------------------
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

// -------------------------
// CORS
// -------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("TmsClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

// -------------------------
// Rate Limiting
// -------------------------
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter
        .Create<HttpContext, string>(httpContext =>
        {
            var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);

            return tier switch
            {
                ApiKeyTier.Paid => RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: $"paid:{partitionKey}",
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 200,
                        TokensPerPeriod = 100,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(100000),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }),

                ApiKeyTier.Free => RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: $"free:{partitionKey}",
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 30,
                        TokensPerPeriod = 10,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(100000),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }),

                _ => RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: $"anon:{partitionKey}",
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 10,
                        TokensPerPeriod = 5,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(10000),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    })
            };
        });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, ct) =>
    {
        var retryAfter = "100000";
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ts))
            retryAfter = ((int)ts.TotalSeconds).ToString();

        context.HttpContext.Response.Headers.RetryAfter = retryAfter;
        context.HttpContext.Response.ContentType = "application/problem+json";

        await context.HttpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Title = "Rate limit exceeded",
                Detail = $"Too many requests. Retry after {retryAfter} seconds.",
                Status = StatusCodes.Status429TooManyRequests,
                Type = "https://tms.local/errors/rate_limit_exceeded"
            }, ct);
    };

    options.AddConcurrencyLimiter("transcripts", opt =>
    {
        opt.PermitLimit = 5;
        opt.QueueLimit = 20;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.AddTokenBucketLimiter("search", opt =>
    {
        opt.TokenLimit = 10;
        opt.TokensPerPeriod = 5;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        opt.QueueLimit = 2;
    });
});

// -------------------------
// Exercise 5: Channel + Status Store + Worker
// -------------------------
builder.Services.AddSingleton(Channel.CreateBounded<TranscriptRequest>(
    new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.Wait
    }));
builder.Services.AddSingleton<ITranscriptStatusStore, InMemoryTranscriptStatusStore>();

// -------------------------
// Exercise 6: SignalR
// -------------------------
builder.Services.AddSignalR();

// Wire the notify delegate — keeps Infrastructure free of Api references
builder.Services.AddSingleton<Func<string, string, string, Task>>(sp =>
{
    var hub = sp.GetRequiredService(
        typeof(IHubContext<TmsHub, ITmsHubClient>))
        as IHubContext<TmsHub, ITmsHubClient>;

    return (studentId, reportId, downloadUrl) =>
        hub!.Clients
            .Group($"student-{studentId}")
            .ReceiveTranscriptReady(reportId, downloadUrl);
});

builder.Services.AddHostedService<TranscriptWorker>();
// -------------------------
// Health Checks
// -------------------------
builder.Services.AddHealthChecks();

// =========================================================
var app = builder.Build();
// =========================================================

if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi("/openapi/v1.json");
    // app.MapOpenApi("/openapi/v2.json");
app.MapOpenApi("/openapi/{documentName}.json");
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("TMS API Explorer")
            .WithTheme(ScalarTheme.DeepSpace)
            .AddDocument("v1", "/openapi/v1.json")
            .AddDocument("v2", "/openapi/v2.json");
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseMiddleware<V1DeprecationMiddleware>();
app.UseCors("TmsClient");

// Seed — Development only
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
}

// ── Endpoints ─────────────────────────────────────────────
app.MapHub<TmsHub>("/hubs/tms");                              // Exercise 6
app.MapHealthChecks("/health/live").DisableRateLimiting();
app.MapHealthChecks("/health/ready").DisableRateLimiting();
app.MapControllers();

app.Run();// 
