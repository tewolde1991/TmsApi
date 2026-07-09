using TmsApi.Dtos;
using TmsApi.DTOs;
namespace TmsApi.Services;

public interface IEnrollmentService
{
  Task<EnrollmentResponseDto?> GetByIdAsync(
      int courseId, int id, CancellationToken ct);

  Task<EnrollmentResponseDto> CreateAsync(
      int courseId,
      EnrollStudentRequest request,
      CancellationToken ct);
  void GetAllAsync();
}