// // using System.Collections.Concurrent;

// // public class CourseService(ILogger<CourseService> logger) : ICourseService
// // {
// //   private readonly ConcurrentDictionary<string, Course> _courses = new();

// //   public Task<IEnumerable<Course>> GetAllAsync()
// //   {
// //     return Task.FromResult(_courses.Values.AsEnumerable());
// //   }

// //   public Task<Course?> GetByIdAsync(string id)
// //   {
// //     if (_courses.TryGetValue(id, out var course))
// //     {
// //       return Task.FromResult<Course?>(course);
// //     }

// //     logger.LogWarning("Course {Id} not found", id);
// //     return Task.FromResult<Course?>(null);
// //   }

// //   public Task<Course> CreateAsync(string courseCode, string title, int credits)
// //   {
// //     var id = Guid.NewGuid().ToString("N")[..8];
// //     var course = new Course(id, courseCode, title, credits);
// //     _courses[id] = course;

// //     logger.LogInformation("Course {Id} created", id);
// //     return Task.FromResult(course);
// //   }

// //   public Task<bool> DeleteAsync(string id)
// //   {
// //     var removed = _courses.TryRemove(id, out _);
// //     if (!removed)
// //     {
// //       logger.LogWarning("Course {Id} not found for deletion", id);
// //     }
// //     return Task.FromResult(removed);
// //   }
// // }

// // Module 6
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

public class CourseService(
    TmsDbContext context,
    ILogger<CourseService> logger) : ICourseService
{
  // ── GetByIdAsync ─────────────────────────────────────
  // AsNoTracking → read-only, no EF memory overhead
  // Select → projection at DB level, not in C# memory
  // c.Enrollments.Count → SQL COUNT(*) subquery
  public Task<CourseResponseDto?> GetByIdAsync(
      int id, CancellationToken ct) =>
      context.Courses
          .AsNoTracking()
          .Where(c => c.Id == id)
          .Select(c => new CourseResponseDto(
              c.Id,
              c.Code,
              c.Title,
              c.MaxCapacity,
              c.Enrollments.Count))   // SQL COUNT(*)
          .FirstOrDefaultAsync(ct);  // null if not found

  // ── CreateAsync ───────────────────────────────────────
  // DTO in → Entity insert → re-query → DTO out
  public async Task<CourseResponseDto> CreateAsync(
      CreateCourseRequest request,
      CancellationToken ct)
  {
    // ① DTO → Entity mapping
    var course = new Course
    {
      Code = request.Code,
      Title = request.Title,
      MaxCapacity = request.MaxCapacity
    };

    // ② INSERT SQL
    context.Courses.Add(course);
    await context.SaveChangesAsync(ct);

    // ③ Log — 1 per write, never per read
    logger.LogInformation(
        "Created course {CourseId} ({Code})",
        course.Id, course.Code);

    // ④ Re-query → fresh DTO with EnrollmentCount=0
    return (await GetByIdAsync(course.Id, ct))!;
    // null! safe — we just inserted it
  }

  // ── CodeExistsAsync ───────────────────────────────────
  // AnyAsync → SELECT EXISTS (LIMIT 1) — fastest check
  public Task<bool> CodeExistsAsync(
      string code, CancellationToken ct) =>
      context.Courses
          .AsNoTracking()
          .AnyAsync(c => c.Code == code, ct);
}