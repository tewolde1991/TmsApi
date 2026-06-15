

// // exercise 1
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Register authentication & authorization services into the correct collection
builder.Services.AddAuthentication("TrainingAuth")
.AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("TrainingAuth", null);
builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware order: Routing → Authentication → Authorization
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Protected endpoint
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization();

app.Run();
// // // app.Use(async (context, next) =>
// // // {
// // //     // Code BEFORE next middleware
// // //     Console.WriteLine("Before next middleware");
// // //     await next.Invoke(context);  // ← KEY LINE
// // //                                  // Code AFTER next middleware
// // //     Console.WriteLine("After next middleware");
// // // });
// // // app.Run();

// // // app.Run(async (context) =>
// // // {
// // //     // Code BEFORE next middleware
// // //     Console.WriteLine("Before next middleware");
// // //     await context.Response.WriteAsync("Hello, World!");  // ← KEY LINE
// // //                                  // Code AFTER next middleware
// // //     Console.WriteLine("After next middleware");
// // // });

// // exercise 2
// using Microsoft.AspNetCore.Authentication;

// var builder = WebApplication.CreateBuilder(args);

// // Transient: new instance every time
// builder.Services.AddTransient<IGradeCalculator, GradeCalculator>();

// // Scoped: one instance per HTTP request
// builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// // Singleton: one instance for the whole application
// builder.Services.AddSingleton<IConfigReader, ConfigReader>();

// // Add services for authentication (training handler)
// builder.Services.AddAuthentication("Training")
//     .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
// builder.Services.AddAuthorization();
// // add buggy registrations
// builder.Services.AddSingleton<EnrollmentWorker>();
// builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
// // add host validation
// builder.Host.UseDefaultServiceProvider(options =>
// {
//     options.ValidateScopes = true;
//     options.ValidateOnBuild = true;
// });
// var app = builder.Build();


// // 1. Custom logging middleware FIRST (wraps everything)
// app.UseMiddleware<RequestLoggingMiddleware>();

// // 2. Exception handler (so errors also get logged and return ProblemDetails later)
// app.UseExceptionHandler(exceptionHandlerApp =>
// {
//     exceptionHandlerApp.Run(async context =>
//     {
//         context.Response.StatusCode = 500;
//         await context.Response.WriteAsync("An error occured");
//     });
// });

// // // 3. Standard middleware
// app.UseHttpsRedirection();
// app.UseRouting();

// // 4. Authentication & Authorization (still before endpoints)
// app.UseAuthentication();
// app.UseAuthorization();

// // 5. Protected endpoint
// app.MapGet("/api/assesments/results/", () => Results.Ok(new
// {
//     courseCode = "CS-101",
//     studentId = "S-001",
//     letterGrade = "A"
// })).RequireAuthorization();

// // app.Run();


// using Microsoft.AspNetCore.Authentication;
// using Microsoft.EntityFrameworkCore;
// using TmsApi;
// using TmsApi.Data;
// using TmsApi.Services;

// var builder = WebApplication.CreateBuilder(args);

// // ========== 1. Add DbContext (Scoped by default) ==========
// builder.Services.AddDbContext<TMSDbContext>(options =>
//     options.UseInMemoryDatabase("TmsMemoryDb"));   // In-memory for testing

// // ========== 2. Register services with CORRECT lifetimes ==========
// builder.Services.AddSingleton
// <IEnrollmentService, EnrollmentService>();      // Scoped (uses DbContext)
// builder.Services.AddTransient<IGradeCalculator, GradeCalculator>();       // Transient (stateless)
// builder.Services.AddSingleton<IConfigReader, ConfigReader>();             // Singleton (immutable config)

// // ========== 3. Background worker (Singleton) – no captive dependency now ==========
// builder.Services.AddHostedService<EnrollmentWorker>();

// // ========== 4. Authentication (from Session 1) ==========
// builder.Services.AddAuthentication("Training")
//     .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
// builder.Services.AddAuthorization();

// // ========== 5. Enable validation to catch lifetime mistakes ==========
// builder.Host.UseDefaultServiceProvider(options =>
// {
//     options.ValidateScopes = true;   // Catches Singleton -> Scoped
//     options.ValidateOnBuild = true;  // Catches missing registrations
// });

// // ========== 6. Add controllers (if you have any) ==========
// builder.Services.AddControllers();

// var app = builder.Build();

// // ========== Middleware pipeline (from Session 1) ==========
// app.UseMiddleware<RequestLoggingMiddleware>();
// app.UseExceptionHandler(exceptionHandlerApp =>
// {
//     exceptionHandlerApp.Run(async context =>
//     {
//         context.Response.StatusCode = 500;
//         await context.Response.WriteAsync("An error occurred");
//     });
// });
// app.UseHttpsRedirection();
// app.UseRouting();
// app.UseAuthentication();
// app.UseAuthorization();

// // ========== 7. Endpoints ==========
// app.MapGet("/api/assessments/results", () => Results.Ok(new
// {
//     courseCode = "CS-101",
//     studentId = "S-001",
//     letterGrade = "A"
// })).RequireAuthorization();

// app.MapControllers();  // if you have any controllers

// app.Run();

