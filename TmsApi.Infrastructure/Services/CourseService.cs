
// // Module 6
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Dtos;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;
// using Microsoft.Extensions.Logging;

namespace TmsApi.Infrastructure.Services;

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

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct)
    {
        // TODO 1: IQueryable — no tracking
        IQueryable<Course> query = context.Courses.AsNoTracking();

        // TODO 2: Search filter — ILike = case-insensitive (PostgreSQL)
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(c =>
                EF.Functions.ILike(c.Title, $"%{request.Search}%") ||
                EF.Functions.ILike(c.Code, $"%{request.Search}%"));

        // TODO 3: Count BEFORE paging — total rows
        var totalCount = await query.CountAsync(ct);
        // ↑ SELECT COUNT(*) — must be before Skip/Take!

        // TODO 4: OrderBy — whitelist only (no arbitrary string in LINQ)
        query = request.OrderBy switch
        {
            "Code" => request.Descending
                              ? query.OrderByDescending(c => c.Code)
                              : query.OrderBy(c => c.Code),
            "MaxCapacity" => request.Descending
                              ? query.OrderByDescending(c => c.MaxCapacity)
                              : query.OrderBy(c => c.MaxCapacity),
            _ => request.Descending   // default: Title
                              ? query.OrderByDescending(c => c.Title)
                              : query.OrderBy(c => c.Title)
        };

        // TODO 5 + 6: Skip/Take + Select + Materialise
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)  // OFFSET
            .Take(request.PageSize)                          // LIMIT
            .Select(c => new CourseResponseDto(
                c.Id, c.Code, c.Title, c.MaxCapacity,
                c.Enrollments.Count))                        // COUNT subquery
            .ToListAsync(ct);

        // TODO 6: Return PagedResponse
        return new PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

}