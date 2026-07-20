using TmsApi.Application.Dtos;

namespace TmsApi.Infrastructure.Services;

public interface IEnrollmentService
{
    Task<EnrollmentResponseDto?> GetByIdAsync(
        int courseId, int id, CancellationToken ct);

    Task<EnrollmentResponseDto> CreateAsync(
        int courseId,
        EnrollmentStudentRequest request,
        CancellationToken ct);
    void GetAllAsync();
    Task<List<EnrollmentResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct);
}