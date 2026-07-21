using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.Dtos;
using TmsApi.Infrastructure.Caching;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class CachedCourseService(
    TmsDbContext context,
    HybridCache cache,
    ILogger<CachedCourseService> logger) : ICachedCourseService
{
    public async Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var key = CacheKeys.Course(id);
        logger.LogInformation("Course cache lookup for {CourseId} using key {Key}", id, key);

        return await cache.GetOrCreateAsync(
            key,
            async token =>
            {
                logger.LogInformation("Course cache miss for {CourseId}", id);
                return await context.Courses
                    .AsNoTracking()
                    .Where(c => c.Id == id)
                    .Select(c => new CourseResponseDto(
                        c.Id,
                        c.Code,
                        c.Title,
                        c.MaxCapacity,
                        c.Enrollments.Count))
                    .FirstOrDefaultAsync(token);
            },
            cancellationToken: ct,
            tags: [CacheKeys.CoursesTag]);
    }

    public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct)
    {
        var key = $"{CacheKeys.CoursesAll}:{request.Page}:{request.PageSize}:{request.Search ?? string.Empty}:{request.OrderBy ?? string.Empty}:{request.Descending}";
        logger.LogInformation("Course list cache lookup using key {Key}", key);

        return await cache.GetOrCreateAsync(
            key,
            async token =>
            {
                logger.LogInformation("Course list cache miss for query {QueryKey}", key);

                IQueryable<TmsApi.Domain.Entities.Course> query = context.Courses.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(request.Search))
                {
                    query = query.Where(c =>
                        EF.Functions.ILike(c.Title, $"%{request.Search}%") ||
                        EF.Functions.ILike(c.Code, $"%{request.Search}%"));
                }

                var totalCount = await query.CountAsync(token);
                query = request.OrderBy switch
                {
                    "Code" => request.Descending ? query.OrderByDescending(c => c.Code) : query.OrderBy(c => c.Code),
                    "MaxCapacity" => request.Descending ? query.OrderByDescending(c => c.MaxCapacity) : query.OrderBy(c => c.MaxCapacity),
                    _ => request.Descending ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title)
                };

                var items = await query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(c => new CourseResponseDto(c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
                    .ToListAsync(token);

                return new PagedResponse<CourseResponseDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            },
            cancellationToken: ct,
            tags: [CacheKeys.CoursesTag]);
    }

    public async Task InvalidateCourseCacheAsync(CancellationToken ct)
    {
        logger.LogInformation("Invalidating course cache entries for tag {Tag}", CacheKeys.CoursesTag);
        await cache.RemoveAsync(CacheKeys.CoursesAll, cancellationToken: ct);
    }
}
