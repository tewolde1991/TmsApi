// public interface ICourseService
// {
//   Task<IEnumerable<Course>> GetAllAsync();
//   Task<Course?> GetByIdAsync(string id);
//   Task<Course> CreateAsync(string courseCode, string title, int credits);
//   Task<bool> DeleteAsync(string id);
// }

// Module 6

using TmsApi.Entities;
namespace TmsApi.Services;

public interface ICourseService
{
  Task<Course?> GetByIdAsync(int id, CancellationToken ct);
  Task<Course> CreateAsync(Course course, CancellationToken ct);
}
