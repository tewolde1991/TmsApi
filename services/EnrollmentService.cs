// public interface IEnrollmentService
// {
//   Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
//   Task<EnrollmentRecord?> GetByIdAsync(string id);
//   Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync();
//   Task<bool> DeleteAsync(string id);
// }

// public class EnrollmentService : IEnrollmentService
// {
//   private readonly Dictionary<string, EnrollmentRecord> _store = new();
//   private readonly ILogger<EnrollmentService> _logger;

//   public EnrollmentService(ILogger<EnrollmentService> logger)
//   {
//     _logger = logger;
//   }

//   public Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
//   {
//     var existing = _store.Values
//         .FirstOrDefault(e => e.StudentId == studentId && e.CourseCode == courseCode);

//     if (existing is not null)
//     {
//       _logger.LogWarning(
//           "Duplicate enrollment attempt {StudentId} already in {CourseCode} (record {EnrollmentId})",
//           studentId, courseCode, existing.Id);
//       return Task.FromResult(existing);
//     }

//     var id = Guid.NewGuid().ToString("N")[..8];
//     var record = new EnrollmentRecord(id, studentId, courseCode, DateTime.UtcNow);
//     _store[id] = record;
//     _logger.LogInformation(
//         "Enrolled {StudentId} in {CourseCode} record {EnrollmentId}",
//         studentId, courseCode, id);
//     return Task.FromResult(record);
//   }

//   public Task<EnrollmentRecord?> GetByIdAsync(string id)
//   {
//     _store.TryGetValue(id, out var record);

//     if (record is null)
//     {
//       _logger.LogWarning("Enrollment {EnrollmentId} not found", id);
//     }

//     return Task.FromResult(record);
//   }

//   public Task<IReadOnlyList<EnrollmentRecord>> GetAllAsync()
//   {
//     IReadOnlyList<EnrollmentRecord> all = _store.Values.ToList();
//     return Task.FromResult(all);
//   }

//   public Task<bool> DeleteAsync(string id)
//   {
//     var removed = _store.Remove(id);

//     if (removed)
//       _logger.LogInformation("Deleted enrollment {EnrollmentId}", id);
//     else
//       _logger.LogWarning("Delete failed enrollment {EnrollmentId} not found", id);

//     return Task.FromResult(removed);
//   }
// }

// public record EnrollmentRecord(
//     string Id, string StudentId, string CourseCode, DateTime EnrolledAt);

// public class TmsDatabaseException(string message) : Exception(message);

// Module 6 exercise 3


using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.DTOs;
using TmsApi.Entities;

namespace TmsApi.Services;

public class EnrollmentService(
    TmsDbContext context,
    ILogger<EnrollmentService> logger) : IEnrollmentService
{
  // GetByIdAsync — read enrollment by id + courseId
  public Task<EnrollmentResponseDto?> GetByIdAsync(
      int courseId,
            int id, CancellationToken ct) =>
      context.Enrollments
          .AsNoTracking()
          .Where(e => e.Id == id && e.CourseId == courseId)
          .Select(e => new EnrollmentResponseDto(
              e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
          .FirstOrDefaultAsync(ct);

  // TODO 2: CreateAsync — insert + save + log + re-query
  public async Task<EnrollmentResponseDto> CreateAsync(
      int courseId,
      EnrollStudentRequest request,
      CancellationToken ct)
  {
    // ① create entity
    var enrollment = new Enrollment
    {
      CourseId = courseId,
      StudentId = request.StudentId,
      EnrolledAt = DateTime.UtcNow   // timestamp without timezone
    };

    // ② INSERT SQL
    context.Enrollments.Add(enrollment);
    await context.SaveChangesAsync(ct);

    // ③ log
    logger.LogInformation(
        "Enrolled student {S} in course {C}",
        request.StudentId, courseId);

    // ④ re-query → fresh DTO
    return (await GetByIdAsync(courseId, enrollment.Id, ct))!;

  }

  public void GetAllAsync()
  {
    throw new NotImplementedException();
  }
}