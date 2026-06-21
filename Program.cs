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
using Scalar.AspNetCore;
using TmsApi;
using Microsoft.AspNetCore.OpenApi;
using TmsApi.Services;

using Microsoft.AspNetCore.Authentication;
using TmsApi.Data;
using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddOpenApi(); // Required before MapOpenApi() will work


// Transient: new instance every time
// builder.Services.AddTransient<IGradeCalculator, GradeCalculator>();

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

// register TmsDbContext scoped for incomming http requests
builder.Services.AddDbContext<TmsDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
.LogTo(Console.WriteLine, LogLevel.Information) // log sql to output window
.EnableSensitiveDataLogging());   // show parametrs in query log 

// add host validation
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

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
app.MapControllers();








using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    context.Database.Migrate(); // // Applies any pending migrations; keeps migration history intact


    if (!context.Students.Any())
    {
        var students = new List<Student>
    {
        new() { RegistrationNumber = "TMS-2026-001", Name = "Alice Smith", GPA = 3.8m, IsActive = true },
        new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones", GPA = 2.9m, IsActive = true },
        new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m, IsActive = false },
        new() { RegistrationNumber = "TMS-2026-0004", Name = "DianaPrince", GPA = 3.9m, IsActive = true },
        new() { RegistrationNumber = "TMS-2026-0005", Name = "EvanWright", GPA = 2.5m, IsActive = true }
        };
        context.Students.AddRange(students);

        var courses = new List<Course>
        {
            new() { Code = "CS-101", Title = "Introduction to ComputerScience", Capacity = 30 },
            new() { Code = "CS-201", Title = "Data Structures and Algorithms", Capacity = 25 },
            new() { Code = "MAT-101", Title = "Calculus I", Capacity =40 }
        };
        context.Courses.AddRange(courses);
        context.SaveChanges();

        var enrollments = new List<Enrollment>
        {
            new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
            new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
            new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
            new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
        };

        context.Enrollments.AddRange(enrollments);
        context.SaveChanges();
    }

}

// 5. Protected endpoint
app.MapGet("/api/assesments/results/", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization();

app.Run();
