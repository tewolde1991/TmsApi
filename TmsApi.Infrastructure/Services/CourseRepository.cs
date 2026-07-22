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

    public async Task UpdateAsync(Course course, CancellationToken ct)
    {
        _context.Courses.Update(course);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct)
    {
        return await _context.Courses.CountAsync(ct);
    }

    public async Task<Course?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<IReadOnlyList<Course>> SearchAsync(string? term, CancellationToken ct)
    {
        var query = _context.Courses
            .AsNoTracking()
            .Include(c => c.Enrollments)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(term))
        {
            term = term.Trim();
           query = query.Where(c =>
               EF.Functions.ILike(c.Title, $"%{term}%") ||
               EF.Functions.ILike(c.Code, $"%{term}%"));
        }

        return await query
            .OrderBy(c => c.Title)
            .Take(50)
            .ToListAsync(ct);
    }
}