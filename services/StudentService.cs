// using System.Collections.Concurrent;

// public class StudentService(ILogger<StudentService> logger) : IStudentService
// {
//   private readonly ConcurrentDictionary<string, Student> _students = new();

//   public Task<IEnumerable<Student>> GetAllAsync()
//   {
//     return Task.FromResult(_students.Values.AsEnumerable());
//   }

//   public Task<Student?> GetByIdAsync(string id)
//   {
//     if (_students.TryGetValue(id, out var student))
//     {
//       return Task.FromResult<Student?>(student);
//     }

//     logger.LogWarning("Student {Id} not found", id);
//     return Task.FromResult<Student?>(null);
//   }

//   public Task<Student> CreateAsync(string firstName, string lastName, string email)
//   {
//     var id = Guid.NewGuid().ToString("N")[..8];
//     var student = new Student(id, firstName, lastName, email);
//     _students[id] = student;

//     logger.LogInformation("Student {Id} created", id);
//     return Task.FromResult(student);
//   }

//   public Task<bool> DeleteAsync(string id)
//   {
//     var removed = _students.TryRemove(id, out _);
//     if (!removed)
//     {
//       logger.LogWarning("Student {Id} not found for deletion", id);
//     }
//     return Task.FromResult(removed);
//   }
// }