using TmsApi.Application.Dtos;

namespace TmsApi.Infrastructure.Services;

public interface ICachedCourseService
{
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct);
    Task InvalidateCourseCacheAsync(CancellationToken ct);
}