using TmsApi.Application.DTOs;

namespace TmsApi.Infrastructure.Services;

public interface IStudentService
{
    Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<StudentDetailDto?> GetDetailByIdAsync(int id, CancellationToken ct);
    Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(PagedRequest request, CancellationToken ct);
    Task<StudentResponseDto> CreateAsync(CreateStudentRequest request, CancellationToken ct);
    Task<StudentResponseDto> UpdateAsync(int id, UpdateStudentRequest request, CancellationToken ct);
    Task<bool> RegistrationNumberExistsAsync(string registrationNumber, CancellationToken ct);
}