using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class CourseRepository : ICourseRepository
{
    private readonly TmsDbContext _context;

    public CourseRepository(TmsDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetByCodeAsync(string courseCode, CancellationToken ct)
    {
        return await _context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Code == courseCode, ct);
    }

    public async Task<IReadOnlyList<Course>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken ct)
    {
        return await _context.Courses
            .Include(c => c.Enrollments)
            .OrderBy(c => c.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct)
    {
        return await _context.Courses.CountAsync(ct);
    }

    public Task<IReadOnlyList<Course>> GetPagedWithEnrollmentsAsync(int page, int pageSize, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Course> AddAsync(Course course, CancellationToken ct)
    {
        await _context.Courses.AddAsync(course, ct);
        await _context.SaveChangesAsync(ct);
        return course;
    }
}