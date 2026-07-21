
//  Module 7 Ex3
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

app.MapControllers();
app.Run();