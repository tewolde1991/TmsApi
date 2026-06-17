using Microsoft.AspNetCore.Authentication;
// using Microsoft.EntityFrameworkCore;
using TmsApi;
// using TmsApi.Data;
// using TmsApi.Services;
//m4 s2 this up...
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();
// .. this m4 s2
// ========== 1. Add DbContext (Scoped by default) ==========
// builder.Services.AddDbContext<TMSDbContext>(options =>
// options.UseInMemoryDatabase("TmsMemoryDb"));   // In-memory for testing

// ========== 2. Register services with CORRECT lifetimes ==========
builder.Services.AddSingleton
<IEnrollmentService, EnrollmentService>();      // Scoped (uses DbContext)
// builder.Services.AddTransient<IGradeCalculator, GradeCalculator>();       // Transient (stateless)
// builder.Services.AddSingleton<IConfigReader, ConfigReader>();             // Singleton (immutable config)

// ========== 3. Background worker (Singleton) – no captive dependency now ==========
// builder.Services.AddHostedService<EnrollmentWorker>();

// ========== 4. Authentication (from Session 1) ==========
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();

// ========== 5. Enable validation to catch lifetime mistakes ==========
builder.Host.UseDefaultServiceProvider(options =>
{
  options.ValidateScopes = true;   // Catches Singleton -> Scoped
  options.ValidateOnBuild = true;  // Catches missing registrations
});

// ========== 6. Add controllers (if you have any) ==========
builder.Services.AddControllers();
builder.Services.AddSingleton<IStudentService, StudentService>();
// builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
var app = builder.Build();

// ========== Middleware pipeline (from Session 1) ==========
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler(exceptionHandlerApp =>
{
  exceptionHandlerApp.Run(async context =>
  {
    context.Response.StatusCode = 500;
    await context.Response.WriteAsync("An error occurred");
  });
});
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ========== 7. Endpoints ==========
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
  courseCode = "CS-101",
  studentId = "S-001",
  letterGrade = "A"
})).RequireAuthorization();

app.MapControllers();  // if you have any controllers

// app.Run();

// m4-s2
// var builder = WebApplication.CreateBuilder(args);

// ... ሌሎች builder.Services lines (Session 1 ካለ) ...

// builder.Services.AddSingleton<EnrollmentWorker>();
// builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// builder.Host.UseDefaultServiceProvider(options =>
// {
//   options.ValidateScopes = true;
//   options.ValidateOnBuild = true;
// });

// var app = builder.Build();
// app.MapPost("/api/enrollments/", async (IEnrollmentService svc) =>
// {
//   await svc.EnrollAsync("S-001", "CS-101");      // → Information
//   await svc.EnrollAsync("S-001", "CS-101");      // → Warning (duplicate)
//   await svc.GetByIdAsync("does-not-exist");      // → Warning (not found)
//   return Results.Ok("done");
// });
app.Run();