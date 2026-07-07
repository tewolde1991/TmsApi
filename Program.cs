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



// // // M5-S1
// using Microsoft.EntityFrameworkCore;
// using TmsApi.Data;
// using TmsApi.Entities;
// using TmsApi.Services;

// var builder = WebApplication.CreateBuilder(args);
// builder.Services.AddScoped<DashboardService>();
// builder.Services.AddScoped<EnrollmentReportService>();
// builder.Services.AddScoped<StudentUpdateService>();
// builder.Services.AddScoped<ArchiveService>();
// builder.Services.AddControllers();


// builder.Services.AddDbContext<TmsDbContext>(options =>
//     options
//         .UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
//         .LogTo(Console.WriteLine, LogLevel.Information)
//         .EnableSensitiveDataLogging());


// var app = builder.Build();
// app.UseDeveloperExceptionPage();
// // ── Seeder — app ከተሰራ በኋላ ──
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();

//     context.Database.Migrate();

//     if (!context.Students.Any())
//     {
//         var students = new List<Student>
//         {
//             new() { RegistrationNumber = "TMS-2026-0001", Name = "Alice Smith",   Email = "alice.smith@example.com",   GPA = 3.8m, IsActive = true  },
//             new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones",     Email = "bob.jones@example.com",     GPA = 2.9m, IsActive = true  },
//             new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", Email = "charlie.brown@example.com", GPA = 3.4m, IsActive = false },
//             new() { RegistrationNumber = "TMS-2026-0004", Name = "Diana Prince",  Email = "diana.prince@example.com",  GPA = 3.9m, IsActive = true  },
//             new() { RegistrationNumber = "TMS-2026-0005", Name = "Evan Wright",   Email = "evan.wright@example.com",   GPA = 2.5m, IsActive = true  },
//         };
//         context.Students.AddRange(students);

//         var courses = new List<Course>
//         {
//             new() { Code = "CS-101",  Title = "Introduction to Computer Science", MaxCapacity = 30 },
//             new() { Code = "CS-201",  Title = "Data Structures and Algorithms",  MaxCapacity = 25 },
//             new() { Code = "MAT-101", Title = "Calculus I",                       MaxCapacity = 20 },
//         };
//         context.Courses.AddRange(courses);

//         context.SaveChanges(); // ← Students + Courses Ids ያስፈልጋሉ ቀድሞ

//         var enrollments = new List<Enrollment>
//         {
//             new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
//             new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
//             new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
//             new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m },
//         };
//         context.Enrollments.AddRange(enrollments);

//         context.SaveChanges();

//         Console.WriteLine("✅ Database seeded successfully.");
//     }
// }

// app.UseHttpsRedirection();
// app.MapControllers();
// app.Run();

// Module 6

using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<EnrollmentReportService>();
builder.Services.AddScoped<StudentUpdateService>();
builder.Services.AddScoped<ArchiveService>();
builder.Services.AddControllers();


builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<TmsDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase")));
builder.Services.AddControllers();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IStudentService, StudentService>();
var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // removed: extension not found. Install/configure the package that provides this extension if needed.
}
app.MapControllers();
app.Run();
