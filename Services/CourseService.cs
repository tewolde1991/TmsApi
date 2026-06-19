using TmsApi.Entities;

namespace TmsApi;

public interface ICourseService
{
    Task<Course> CourseAsync(string courseCode, int capacity);
    Task<Course?> GetByIdAsync(string id);

    Task<IReadOnlyList<Course>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
    // Task CourseAsync(string courseCode, string capacity, string enrolledAt);
}
// in-memrory implemetnrtaionm
public class CourseService : ICourseService
{
    private readonly Dictionary<string, Course> _store = new();

    private readonly ILogger<CourseService> _logger;
    public CourseService(ILogger<CourseService> logger)
    {
        _logger = logger;
    }
    public async Task<Course> CourseAsync(string courseCode, int Capacity)
    {
        var record = new Course()
        {
            Code = courseCode,
            Capacity = Capacity


        };
        // store records
        _store[courseCode] = record;

        _logger.LogInformation(
                "Course {CourseCode} is created", courseCode
            );
        return await Task.FromResult(record);
    }

    public async Task<Course?> GetByIdAsync(string id)
    {
        // retrive the stired
        _store.TryGetValue(id, out var record);
        return await Task.FromResult(record);
    }
    public async Task<IReadOnlyList<Course>> GetAllAsync()
    {
        // return actual list
        IReadOnlyList<Course> all = _store.Values.ToList();
        await Task.Delay(1);
        return new List<Course>();

    }
    public async Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);
        return await Task.FromResult(removed);
    }

    public Task<Course> CorseAsync(string courseCode)
    {
        throw new NotImplementedException();
    }
}