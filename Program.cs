// exercise 1
// IServiceCollection services = new ServiceCollection();
// var builder = WebApplication.CreateBuilder(args);
// // builder.Services.AddControllers();
// var app = builder.Build();

// //TODO1:Register routing in the pipeline where it belongs for your app.
// app.UseRouting();
// services.AddAuthentication();
// services.AddAuthorization();
// app.MapGet("/api/assesments/results", () => Results.Ok(new
// {
//     courseCode = "TMS101",
//     studentId = "S-001",
//     letterGrade = "A",
// })
// );
// app.Run();
// // app.Use(async (context, next) =>
// // {
// //     // Code BEFORE next middleware
// //     Console.WriteLine("Before next middleware");
// //     await next.Invoke(context);  // ← KEY LINE
// //                                  // Code AFTER next middleware
// //     Console.WriteLine("After next middleware");
// // });
// // app.Run();

// // app.Run(async (context) =>
// // {
// //     // Code BEFORE next middleware
// //     Console.WriteLine("Before next middleware");
// //     await context.Response.WriteAsync("Hello, World!");  // ← KEY LINE
// //                                  // Code AFTER next middleware
// //     Console.WriteLine("After next middleware");
// // });

// exercise 2 module 4 session -2
using TmsApi;
using TmsApi.Services;

using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Transient: new instance every time
builder.Services.AddTransient<IGradeCalculator, GradeCalculator>();

// Scoped: one instance per HTTP request
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Singleton: one instance for the whole application
builder.Services.AddSingleton<IConfigReader, ConfigReader>();

// Add services for authentication (training handler)
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
// add buggy registrations
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
// add host validation
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
var app = builder.Build();


// 1. Custom logging middleware FIRST (wraps everything)
app.UseMiddleware<RequestLoggingMiddleware>();

// 2. Exception handler (so errors also get logged and return ProblemDetails later)
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An error occured");
    });
});

// // 3. Standard middleware
app.UseHttpsRedirection();
app.UseRouting();

// 4. Authentication & Authorization (still before endpoints)
app.UseAuthentication();
app.UseAuthorization();

// 5. Protected endpoint
app.MapGet("/api/assesments/results/", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization();

app.Run();
