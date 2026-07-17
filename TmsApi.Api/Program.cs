
using Scalar.AspNetCore;
using TmsApi.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using TmsApi.Api.Middlewares;
using TmsApi.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// builder.Services.AddOpenApi(); 


builder.Services.AddProblemDetails();
// builder.Services.AddControllers(options =>
// {
//     // options.Filters.Add<AuditLogFilter>();
// });

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
    new HeaderApiVersionReader("X-Api-Version"));
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddOpenApi(documentName:"v1",configureOptions:options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v1";
});
builder.Services.AddOpenApi(documentName:"v2",configureOptions:options =>
{
    options.ShouldInclude = description =>
        description.GroupName == "v2";
});

// register TmsDbContext scoped for incomming http requests
builder.Services.AddDbContext<TmsDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
.LogTo(Console.WriteLine, LogLevel.Information) 
.EnableSensitiveDataLogging());    
builder.Services.AddControllers();
builder.Services.AddProblemDetails();


// Transient: new instance every time
// builder.Services.AddTransient<IGradeCalculator, GradeCalculator>();
builder.Services.AddScoped<StudentService>();
// Scoped: one instance per HTTP request
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// Singleton: one instance for the whole application
builder.Services.AddSingleton<IConfigReader, ConfigReader>();

// register course service here
builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICertificateService, CertificateService>();


builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();


// // Add services for authentication (training handler)
// builder.Services.AddAuthentication("Training")
//     .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
// add buggy registrations
// builder.Services.AddSingleton<EnrollmentWorker>();



// add host validation
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
// builder.Services.AddControllers(options =>
// {
//     // options.Filters.Add<AuditLogFilter>();
// });
var app = builder.Build();

// 1. Custom logging middleware FIRST (wraps everything)
// app.UseMiddleware<RequestLoggingMiddleware>();
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
// app.UseHttpsRedirection();
app.UseRouting();

// 4. Authentication & Authorization (still before endpoints)
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<V1DeprecationMiddleware>();

app.MapControllers();



// // 5. Protected endpoint
// app.MapGet("/api/assesments/results/", () => Results.Ok(new
// {
//     courseCode = "CS-101",
//     studentId = "S-001",
//     letterGrade = "A"
// })).RequireAuthorization();


// session 2 Module 6
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    // var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    // await DataSeeder.SeedAsync(context);
}
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi("/api/{documentName}.json");
    app.MapScalarApiReference(configureOptions: options =>
    {
        options.WithTitle("TMS API Reference")
                .WithTheme(ScalarTheme.DeepSpace)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .WithOpenApiRoutePattern("/api/{documentName}.json");    
                
                options
                        .AddDocument("v1",title: "API Version 1.0")
                        .AddDocument("v2",title: "API Version 2.0");
    });
}



app.Run();


