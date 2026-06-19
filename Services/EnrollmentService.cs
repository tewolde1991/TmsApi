
using TmsApi.Entities;

namespace TmsApi;

public interface IEnrollmentService
{
    Task<Enrollment> EnrollAsync(string studentId, string courseCode);
    Task<Enrollment?> GetByIdAsync(string id);
    Task<IReadOnlyList<Enrollment>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
}
//--- The in-memory implementation--
public class EnrollmentService : IEnrollmentService
{
    private readonly Dictionary<string, Enrollment> _store = new();
    // private readonly TMSDbContext _db;
    private readonly ILogger<EnrollmentService> _logger;
    public EnrollmentService(ILogger<EnrollmentService> logger)
    {

        _logger = logger;
    }
    public async Task<Enrollment> EnrollAsync(string studentId, string courseId, decimal grade)
    {
        var id = Guid.NewGuid().ToString("N")[..8];
        var record = new Enrollment()
        {
            StudentId = studentId,
            CourseId = courseId,
            Grade = grade
        };

        // _store[id] = record;
        _logger.LogInformation(
        "Enrolled {StudentId} in {CourseCode} record {EnrollmentId}",
        studentId, courseId, id);
        return await Task.FromResult(record);
    }
    public async Task<Enrollment?> GetByIdAsync(string id)
    {
        // _store.TryGetValue(id, out var record);
        await Task.Delay(1);
        return null;
    }
    public async Task<IReadOnlyList<Enrollment>> GetAllAsync()
    {
        IReadOnlyList<Enrollment> all = _store.Values.ToList();
        await Task.Delay(1);
        return new List<Enrollment>();
    }
    public async Task<bool> DeleteAsync(string id)
    {
        // var removed = _store.Remove(id);
        await Task.Delay(1);
        return true;
    }

    public Task<Enrollment> EnrollAsync(string studentId, string courseCode)
    {
        throw new NotImplementedException();
    }
}
