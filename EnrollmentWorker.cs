// public class EnrollmentWorker(IServiceScopeFactory scopeFactory)
// {
//     public void ProcessBatch()
//     {
//         // Todo create a short lived
//         // Console.WriteLine("HY");
//      using var scope = scopeFactory.CreateScope();

//     //  // TODO3:Resolve the scoped service from the new scope's provider.
//         var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();


//     }
// }
// Your current EnrollmentWorker.cs likely injects IEnrollmentService directly. That is wrong if the worker is a Singleton.
// Change it to use IServiceScopeFactory


using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Services;

namespace TmsApi;

public class EnrollmentWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EnrollmentWorker> _logger;

    public EnrollmentWorker(IServiceScopeFactory scopeFactory, ILogger<EnrollmentWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(30_000, stoppingToken); // run every 30 seconds

            using var scope = _scopeFactory.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<TmsDbContext>();

var student = await db.Students.FirstOrDefaultAsync(stoppingToken);
if (student is null)
{
    _logger.LogWarning("No students found; skipping background enrollment.");
    continue;
}

var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
// var record = await enrollmentService.EnrollAsync(student.Id, "BG-Course", stoppingToken);
        }
    }
}