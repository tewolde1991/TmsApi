using TmsApi.Entities;

public interface IStudentService
{
    Task<Student> StudentAsync(string Id, string Name, int Age, decimal GPA);

    Task<Student?> GetByIdAsync(string id);

    Task<IReadOnlyList<Student>> GetAllAsync();

    Task<bool> DeleteAsync(string id);
    Task StudentAsync(string courseCode, string capacity, string enrolledAt);
}
// memory implementation
public class StudentService : IStudentService
{
    private readonly Dictionary<string, Student> _store = new();

    private readonly ILogger<StudentService> _logger;

    public StudentService(ILogger<StudentService> logger)
    {
        _logger = logger;
    }

    public async Task<Student> StudentAsync(string id, string Name, int Age, decimal GPA)
    {
        // var id =  Guid.NewGuid().ToString("N")[..8];
        var record = new Student()
        {
            RegistrationNumber = Name,
            GPA = GPA,
            Name = Name
        };

            _store[Name] = record;
            _logger.LogInformation(
            "Student {Name} with {id} is registerd.", Name, id
        );
        return await Task.FromResult(record);
    }

    public async Task<Student?> GetByIdAsync(string id)
    {
        // rtrive the store
        _store.TryGetValue(id, out var record);
        return await Task.FromResult(record);
    }

    public async Task<IReadOnlyList<Student>> GetAllAsync()
    {
        IReadOnlyList<Student> all = _store.Values.ToList();
        await Task.Delay(1);
        return new List<Student>();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);
        return await Task.FromResult(removed);

    }

    public Task<Student> StudentAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task StudentAsync(string courseCode, string capacity, string enrolledAt)
    {
        throw new NotImplementedException();
    }
}
