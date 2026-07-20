
//  Module 7
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using Scalar.AspNetCore;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc.Filters;
// using System.Threading.Tasks;
// using FluentValidation;

using TmsApi.Infrastructure.Persistence;
using TmsApi.Infrastructure.Services;
using TmsApi.Api.middileware;
using TmsApi.Infrastructure.Data;
// using TmsApi.Api.Filters;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// Register Services
// -------------------------

builder.Services.AddProblemDetails();

builder.Services.AddControllers(options =>
{
    // options.Filters.Add<AuditLogFilter>();
});

builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase")));

builder.Services.AddScoped<ICourseService, CourseService>();

IServiceCollection serviceCollection2 = builder.Services.AddScoped<IStudentService, StudentService>();

IServiceCollection serviceCollection1 = serviceCollection2;
IServiceCollection serviceCollection = builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

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


builder.Services.AddDbContext<TmsDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));
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
app.UseMiddleware<V1DeprecationMiddleware>();  // ← before MapControllers

// Seed — Development only!
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
}

app.MapControllers();
app.Run();