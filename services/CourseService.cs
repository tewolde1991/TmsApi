using System.Collections.Concurrent;

public class CourseService(ILogger<CourseService> logger) : ICourseService
{
  private readonly ConcurrentDictionary<string, Course> _courses = new();

  public Task<IEnumerable<Course>> GetAllAsync()
  {
    return Task.FromResult(_courses.Values.AsEnumerable());
  }

  public Task<Course?> GetByIdAsync(string id)
  {
    if (_courses.TryGetValue(id, out var course))
    {
      return Task.FromResult<Course?>(course);
    }

    logger.LogWarning("Course {Id} not found", id);
    return Task.FromResult<Course?>(null);
  }

  public Task<Course> CreateAsync(string courseCode, string title, int credits)
  {
    var id = Guid.NewGuid().ToString("N")[..8];
    var course = new Course(id, courseCode, title, credits);
    _courses[id] = course;

    logger.LogInformation("Course {Id} created", id);
    return Task.FromResult(course);
  }

  public Task<bool> DeleteAsync(string id)
  {
    var removed = _courses.TryRemove(id, out _);
    if (!removed)
    {
      logger.LogWarning("Course {Id} not found for deletion", id);
    }
    return Task.FromResult(removed);
  }
}