public class EnrollmentWorker(IServiceScopeFactory scopeFactory)
{
  public void ProcessBatch()
  {
    // TODO 2: short-lived scope
    using var scope = scopeFactory.CreateScope();

    // TODO 3: scoped service ን ከ scope resolve
    var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

    svc.GetAllAsync();
  }
}