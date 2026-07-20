using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence;

public class CourseRepository : ICourseRepository
{
    private readonly TmsDbContext _context;

    public CourseRepository(TmsDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetByCodeAsync(
        string courseCode,
        CancellationToken ct = default)
    {
        return await _context.Courses
            .FirstOrDefaultAsync(
                c => c.Code == courseCode,
                ct);
    }

    public async Task<bool> ExistsAsync(
        string courseCode,
        CancellationToken ct = default)
    {
        return await _context.Courses
            .AnyAsync(
                c => c.Code == courseCode,
                ct);
    }

    public async Task AddAsync(
        Course course,
        CancellationToken ct = default)
    {
        await _context.Courses.AddAsync(course, ct);
    }

    public async Task SaveChangesAsync(
        CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    
    }

public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(
    int studentId,
    CancellationToken ct = default)
{
    return await _context.Enrollments
        .Where(e => e.StudentId == studentId)
        .AsNoTracking()
        .ToListAsync(ct);
}
}