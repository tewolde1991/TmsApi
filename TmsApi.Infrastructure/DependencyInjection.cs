using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Infrastructure;


public static class DependencyInjection {
public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Transient: new instance every time
        // builder.Services.AddTransient<IGradeCalculator, GradeCalculator>();
      services.AddScoped<StudentService>();
        // Scoped: one instance per HTTP request
      services.AddScoped<ICourseRepository, CourseRepository>();
      services.AddScoped<IStudentRepository, StudentRepository>();

        // Singleton: one instance for the whole application
      services.AddSingleton<IConfigReader, ConfigReader>();

        // register course service here
      services.AddScoped<ICourseService, CourseService>();

      services.AddScoped<IStudentService, StudentService>();
      services.AddScoped<ICertificateService, CertificateService>();


      services.AddScoped<ICourseRepository, CourseRepository>();
      services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
      services.AddScoped<IStudentRepository, StudentRepository>();

    }

}

