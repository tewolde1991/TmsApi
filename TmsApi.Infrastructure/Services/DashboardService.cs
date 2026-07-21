using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Dtos;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class DashboardService
{
    private readonly TmsDbContext _db;
    private const int PageSize = 20;

    public DashboardService(TmsDbContext db) => _db = db;

    // TODO 1: Paged students
    public async Task<(List<StudentDto> Rows, int TotalCount)>
        GetPagedStudentsAsync(int page, CancellationToken ct = default)
    {
        if (page < 1) page = 1;

        var query = _db.Students
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new StudentDto(s.Id, s.Name, s.RegistrationNumber));

        var total = await query.CountAsync(ct);
        var rows = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync(ct);

        return (rows, total);
    }

    // TODO 2: Top 5 courses — 2 step approach
    public async Task<List<CourseEnrollmentDto>>
        GetTop5CoursesByEnrollmentAsync(CancellationToken ct = default)
    {
        // Step 1: CourseId + Count ያውጡ
        var counts = await _db.Enrollments
            .AsNoTracking()
            .GroupBy(e => e.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync(ct);

        // Step 2: Course titles ያምጡ
        var courseIds = counts.Select(x => x.CourseId).ToList();
        var courses = await _db.Courses
            .AsNoTracking()
            .Where(c => courseIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Title })
            .ToListAsync(ct);

        // Step 3: Combine
        return counts
            .Join(courses,
                cnt => cnt.CourseId,
                c => c.Id,
                (cnt, c) => new CourseEnrollmentDto(c.Title, cnt.Count))
            .ToList();
    }
}