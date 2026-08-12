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
using Microsoft.Extensions.Logging;
using TmsApi.Application.Dtos;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class EnrollmentService(
    TmsDbContext context,
    ILogger<EnrollmentService> logger) : IEnrollmentService
{
    // GetByIdAsync — read enrollment by id + courseId
    // GetByIdAsync
    public Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId, int id, CancellationToken ct) =>
        context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.Course.Code,                                    // CourseCode
                e.Course.Title,                                   // CourseTitle
                e.StudentId,
                e.Student.FirstName + " " + e.Student.LastName,  // StudentName
                e.IsArchived ? "Archived" : "Active",             // Status
                e.EnrolledAt
            ))
            .FirstOrDefaultAsync(ct);
    // TODO 2: CreateAsync — insert + save + log + re-query
    // GetByCourseAsync
    public Task<List<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId, CancellationToken ct) =>
        context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.Course.Code,
                e.Course.Title,
                e.StudentId,
                e.Student.FirstName + " " + e.Student.LastName,
                e.IsArchived ? "Archived" : "Active",
                e.EnrolledAt
            ))
            .ToListAsync(ct);
    public Task<List<EnrollmentResponseDto>> GetAllAsync(CancellationToken ct = default) =>
     context.Enrollments
         .AsNoTracking()
         .Select(e => new EnrollmentResponseDto(
             e.Id,
             e.CourseId,
             e.Course.Code,
             e.Course.Title,
             e.StudentId,
             e.Student.FirstName + " " + e.Student.LastName,
             e.IsArchived ? "Archived" : "Active",
             e.EnrolledAt
         ))
         .ToListAsync(ct);

    public Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollmentStudentRequest request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
    //     public Task<List<EnrollmentResponseDto>> GetByCourseAsync(
    //         int courseId, CancellationToken ct) =>
    //         context.Enrollments
    //             .AsNoTracking()
    //             .Where(e => e.CourseId == courseId)
    //             .Select(e => new EnrollmentResponseDto(
    //                 e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
    //             .ToListAsync(ct);
    // }
}