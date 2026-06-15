// using TmsApi.Data;

namespace  TmsApi;


public interface IEnrollmentService
{
Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
Task<EnrollmentRecord?> GetByIdAsync(string id);
Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
Task<bool> DeleteAsync(string id);
}
//--- The in-memory implementation--
public class EnrollmentService : IEnrollmentService
{
private readonly Dictionary<string, EnrollmentRecord> _store = new();
// private readonly TMSDbContext _db;
private readonly ILogger<EnrollmentService> _logger;
public EnrollmentService(ILogger<EnrollmentService> logger)
{
   
_logger = logger;
}
public async Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
{
var id = Guid.NewGuid().ToString("N")[..8];
var record = new EnrollmentRecord(id, studentId, courseCode, DateTime.UtcNow);

// _store[id] = record;
_logger.LogInformation(
"Enrolled {StudentId} in {CourseCode} record {EnrollmentId}",
studentId, courseCode, id);
return await Task.FromResult(record);
}
public async Task<EnrollmentRecord?> GetByIdAsync(string id)
{
    // _store.TryGetValue(id, out var record);
    await Task.Delay(1);
    return null;
}
public async Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
{
IReadOnlyList<EnrollmentRecord> all = _store.Values.ToList();
    await Task.Delay(1);
    return new List<EnrollmentRecord>();
}
public async Task<bool> DeleteAsync(string id)
{
// var removed = _store.Remove(id);
    await Task.Delay(1);
    return true;
}
}
