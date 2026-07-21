
// // Module 6
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Dtos;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Caching;
// using Microsoft.Extensions.Logging;

namespace TmsApi.Infrastructure.Services;

public class CourseService(
    TmsDbContext context,
    ILogger<CourseService> logger,
    ICachedCourseService cachedCourseService) : ICourseService
{
    // ── GetByIdAsync ─────────────────────────────────────
    // AsNoTracking → read-only, no EF memory overhead
    // Select → projection at DB level, not in C# memory
    // c.Enrollments.Count → SQL COUNT(*) subquery
    public Task<CourseResponseDto?> GetByIdAsync(
        int id, CancellationToken ct) =>
        cachedCourseService.GetByIdAsync(id, ct);

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

        await cachedCourseService.InvalidateCourseCacheAsync(ct);

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

    public async Task<CourseResponseDto?> UpdateAsync(int id, UpdateCourseRequest request, CancellationToken ct)
    {
        var course = await context.Courses
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync(ct);

        if (course is null)
            return null;

        course.Code = request.Code;
        course.Title = request.Title;
        course.MaxCapacity = request.MaxCapacity;

        await context.SaveChangesAsync(ct);
        await cachedCourseService.InvalidateCourseCacheAsync(ct);

        return await GetByIdAsync(course.Id, ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var course = await context.Courses
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync(ct);

        if (course is null)
            return false;

        context.Courses.Remove(course);
        await context.SaveChangesAsync(ct);
        await cachedCourseService.InvalidateCourseCacheAsync(ct);

        return true;
    }

    public Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct)
    {
        return cachedCourseService.GetCoursesAsync(request, ct);
    }

}