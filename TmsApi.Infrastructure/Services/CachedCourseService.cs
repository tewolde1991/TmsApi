using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Caching;

namespace TmsApi.Infrastructure.Services;

public class CachedCourseService(
    HybridCache cache,
    ICourseRepository repo,
    ILogger<CachedCourseService> logger)
    : ICachedCourseService
{
    public async Task<CourseDetailDto> GetCourseAsync(string code, CancellationToken ct)
    {
        var key = CacheKeys.Course(code);
        var dbHit = false;

        var dto = await cache.GetOrCreateAsync(
            key,
            (repo, code),
            async (state, token) =>
            {
                dbHit = true;
                logger.LogInformation("Cache MISS for {Key} fetching from DB", key);

                var course = await state.repo.GetByCodeAsync(state.code, token)
                             ?? throw new InvalidOperationException($"Course {state.code} not found.");

                return new CourseDetailDto
                {
                    Id = course.Id,
                    Code = course.Code,
                    Title = course.Title,
                    MaxCapacity = course.MaxCapacity,
                    EnrollmentCount = course.Enrollments.Count,
                    Links = Array.Empty<LinkDto>()

                };
            },
            tags: new[] { CacheKeys.CoursesTag },
            cancellationToken: ct);

        if (!dbHit)
        {
            logger.LogInformation("Cache HIT for {Key}", key);
        }

        return dto;
    }

    public async Task<IReadOnlyList<CourseDetailDto>> GetAllCoursesAsync(CancellationToken ct)
    {
        var key = CacheKeys.CoursesAll;
        var dbHit = false;

        var list = await cache.GetOrCreateAsync(
            key,
            repo,
            async (state, token) =>
            {
                dbHit = true;
                logger.LogInformation("Cache MISS for {Key} fetching from DB", key);

                var courses = await state.GetPagedAsync(1, int.MaxValue, token);
                return courses.Select(c => new CourseDetailDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Title = c.Title,
                    MaxCapacity = c.MaxCapacity,
                    EnrollmentCount = c.Enrollments.Count,
                    Links = Array.Empty<LinkDto>()
                }).ToList();
            },
            tags: new[] { CacheKeys.CoursesTag },
            cancellationToken: ct);

        if (!dbHit)
        {
            logger.LogInformation("Cache HIT for {Key}", key);
        }

        return list;
    }

    public async Task InvalidateCourseCacheAsync(CancellationToken ct)
    {
        logger.LogInformation("Invalidating cache tag {Tag}", CacheKeys.CoursesTag);
        await cache.RemoveByTagAsync(CacheKeys.CoursesTag, ct);
    }
}