using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi;

// 1. Interface: signatures only
public interface IEnrollmentService
{
    Task<Enrollment> EnrollAsync(int studentId, string courseCode, CancellationToken ct = default);
    Task<Enrollment?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Enrollment>> GetAllAsync(CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
     Task<int> ArchiveByYearAsync(int year, CancellationToken ct = default);
}

// 2. Implementation: actual logic
public class EnrollmentService : IEnrollmentService
{
    private readonly TmsDbContext _db;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(TmsDbContext db, ILogger<EnrollmentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Enrollment> EnrollAsync(int studentId, string courseCode, CancellationToken ct = default)
    {
        // Find course by code
        var course = await _db.Courses
            .FirstOrDefaultAsync(c => c.Code == courseCode, ct);

        if (course is null)
        {
            throw new InvalidOperationException($"Course with code '{courseCode}' not found.");
        }

        // Check student exists
        var studentExists = await _db.Students
            .AnyAsync(s => s.Id == studentId, ct);

        if (!studentExists)
        {
            throw new InvalidOperationException($"Student with id '{studentId}' not found.");
        }

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = course.Id,
            EnrolledAt = DateTime.UtcNow,
            Year = DateTime.UtcNow.Year,
            IsArchived = false
        };

        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Enrolled student {StudentId} in course {CourseCode} with enrollment {EnrollmentId}",
            studentId, courseCode, enrollment.Id);

        return enrollment;
    }

    public async Task<Enrollment?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<IReadOnlyList<Enrollment>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _db.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .ToListAsync(ct);

        return list;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var enrollment = await _db.Enrollments.FindAsync(new object[] { id }, ct);
        if (enrollment is null)
        {
            return false;
        }
// hard delete
        _db.Enrollments.Remove(enrollment);

        // soft delete
        enrollment.IsArchived = true;  
        await _db.SaveChangesAsync(ct);

        return true;
    }
public async Task<int> ArchiveByYearAsync(int year, CancellationToken ct = default)
    {
var affected = await _db.Enrollments
            .Where(e => e.Year == year && !e.IsArchived)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(e => e.IsArchived, true),
                ct);

        return affected;    }

}