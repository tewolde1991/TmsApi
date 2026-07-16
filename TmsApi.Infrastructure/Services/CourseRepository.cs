using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class CourseRepository(TmsDbContext context) : ICourseRepository
{
    public Task<Course?> GetByCodeAsync(string courseCode, CancellationToken ct)
        => context.Courses
            .Include(c => c.Enrollments)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == courseCode, ct);
}

public class EnrollmentRepository(TmsDbContext context) : IEnrollmentRepository
{
    public Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct)
        => context.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.Course.Code == courseCode, ct);

    public async Task AddAsync(Enrollment enrollment, CancellationToken ct)
    {
        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Enrollment>> GetByStudentIdAsync(
        int studentId,
        CancellationToken ct)
        => await context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .AsNoTracking()
            .ToListAsync(ct);
}