//  exercise 1
/*var builder = WebApplication.CreateBuilder(args);
// registering sevices
builder.Services.AddAuthorization();
builder.Services.AddAuthentication()
            .AddBearerToken();
var app = builder.Build();
//TODO1:Register routing in the pipeline where it belongs for your app.
app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();
// 3. Your custom middleware example
app.Use(async (context, next) =>
{
    Console.WriteLine("Before next middleware");
    await next.Invoke(context);        // ← This calls the next middleware
    Console.WriteLine("After next middleware");
});
app.MapGet("/api/assesments/results", () => Results.Ok(new
{
    courseCode = "TMS101",
    studentId = "S-001",
    letterGrade = "A",
})
).RequireAuthorization();


// 5. Terminal middleware - should be at the very end
app.Run(async (context) =>
{
    await context.Response.WriteAsync("Hello, World! - Fallback");
});
app.Run();
*/
// exercise 2 module 4 session -2
using Scalar.AspNetCore;
using TmsApi;
using TmsApi.Services;

using Microsoft.AspNetCore.Authentication;
using TmsApi.Data;
using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;
using Tms.Api.Filters;

var builder = WebApplication.CreateBuilder(args);
// register TmsDbContext scoped for incomming http requests
builder.Services.AddDbContext<TmsDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
.LogTo(Console.WriteLine, LogLevel.Information) // log sql to output window
.EnableSensitiveDataLogging());   // show parametrs in query log 
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(); // Required before MapOpenApi() will work


// Transient: new instance every time
// builder.Services.AddTransient<IGradeCalculator, GradeCalculator>();
builder.Services.AddScoped<StudentService>();
// Scoped: one instance per HTTP request
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Singleton: one instance for the whole application
builder.Services.AddSingleton<IConfigReader, ConfigReader>();

// register course service here
builder.Services.AddScoped<ICourseService, CourseService>();
// Add services for authentication (training handler)
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
// add buggy registrations
builder.Services.AddSingleton<EnrollmentWorker>();



// add host validation
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
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
app.MapControllers();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


// 5. Protected endpoint
app.MapGet("/api/assesments/results/", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId = "S-001",
    letterGrade = "A"
})).RequireAuthorization();


// session 2 Module 6
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(context);
}

app.Run();
