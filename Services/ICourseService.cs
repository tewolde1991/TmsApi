
using TmsApi.Entities;

namespace TmsApi.Services;

public interface ICourseService
{
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);
    // Task<bool> CodeExistsAsync(string code, CancellationToken ct);

    // why interface defines what the service can do
}