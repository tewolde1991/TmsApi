// using Microsoft.AspNetCore.Authentication;
// using Scalar.AspNetCore;
// using Microsoft.AspNetCore.OpenApi;
// // using Microsoft.EntityFrameworkCore;
// using TmsApi;
// // using TmsApi.Data;
// // using TmsApi.Services;
// //m4 s2 this up...
// var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddOptions<PaymentOptions>()
//     .BindConfiguration("Payments")
//     .ValidateDataAnnotations()
//     .ValidateOnStart();
// // .. this m4 s2
// // ========== 1. Add DbContext (Scoped by default) ==========
// // builder.Services.AddDbContext<TMSDbContext>(options =>
// // options.UseInMemoryDatabase("TmsMemoryDb"));   // In-memory for testing

// // ========== 2. Register services with CORRECT lifetimes ==========
// builder.Services.AddSingleton
// <IEnrollmentService, EnrollmentService>();      // Scoped (uses DbContext)
// // builder.Services.AddTransient<IGradeCalculator, GradeCalculator>();       // Transient (stateless)
// // builder.Services.AddSingleton<IConfigReader, ConfigReader>();             // Singleton (immutable config)

// // ========== 3. Background worker (Singleton) – no captive dependency now ==========
// // builder.Services.AddHostedService<EnrollmentWorker>();

// // ========== 4. Authentication (from Session 1) ==========
// builder.Services.AddAuthentication("Training")
//     .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
// builder.Services.AddAuthorization();


// // ========== 5. Enable validation to catch lifetime mistakes ==========
// builder.Host.UseDefaultServiceProvider(options =>
// {
//   options.ValidateScopes = true;   // Catches Singleton -> Scoped
//   options.ValidateOnBuild = true;  // Catches missing registrations
// });

// // ========== 6. Add controllers (if you have any) ==========
// builder.Services.AddControllers();
// builder.Services.AddProblemDetails();
// builder.Services.AddOpenApi();          // TODO0 (setup) - generates the OpenAPI document
// builder.Services.AddProblemDetails();
// builder.Services.AddSingleton<IStudentService, StudentService>();
// // builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
// builder.Services.AddSingleton<ICourseService, CourseService>();
// var app = builder.Build();

// // ========== Middleware pipeline (from Session 1) ==========
// app.UseMiddleware<RequestLoggingMiddleware>();
// app.UseExceptionHandler(exceptionHandlerApp =>
// {
//   exceptionHandlerApp.Run(async context =>
//   {
//     context.Response.StatusCode = 500;
//     await context.Response.WriteAsync("An error occurred");
//   });
// });
// app.UseExceptionHandler();
// app.UseStatusCodePages();
// app.UseHttpsRedirection();
// app.UseRouting();
// app.UseAuthentication();
// app.UseAuthorization();

// // ========== 7. Endpoints ==========
// app.MapGet("/api/assessments/results", () => Results.Ok(new
// {
//   courseCode = "CS-101",
//   studentId = "S-001",
//   letterGrade = "A"
// })).RequireAuthorization();

// // app.MapGet("/api/error", () =>
// // {
// //   throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
// // });
// app.MapGet("/api/error", () => { throw new InvalidOperationException("boom"); });
// app.MapControllers();  // if you have any controllers

// // app.Run();

// // m4-s2
// // var builder = WebApplication.CreateBuilder(args);

// // ... ሌሎች builder.Services lines (Session 1 ካለ) ...

// // builder.Services.AddSingleton<EnrollmentWorker>();
// // builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// // builder.Host.UseDefaultServiceProvider(options =>
// // {
// //   options.ValidateScopes = true;
// //   options.ValidateOnBuild = true;
// // });

// // var app = builder.Build();
// // app.MapPost("/api/enrollments/", async (IEnrollmentService svc) =>
// // {
// //   await svc.EnrollAsync("S-001", "CS-101");      // → Information
// //   await svc.EnrollAsync("S-001", "CS-101");      // → Warning (duplicate)
// //   await svc.GetByIdAsync("does-not-exist");      // → Warning (not found)
// //   return Results.Ok("done");
// // });
// // app.Run();

// // ... your existing service registrations ...


// // TODO1: Check environment
// if (app.Environment.IsDevelopment())
// {
//   // TODO2: Dev-only diagnostics
//   app.MapOpenApi();       // exposes /openapi/v1.json
//   app.MapScalarApiReference();   // exposes /scalar/v1 (interactive explorer)
// }
// else
// {
//   // TODO3: Prod-only safety
//   app.UseExceptionHandler();     // catches exceptions, returns ProblemDetails JSON
// }

// app.UseHttpsRedirection();// ... your existing endpoints/middleware ...

// app.Run();



// // M5-S1

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));

var app = builder.Build();
app.Run();