
using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;
namespace TmsApi.Application.Interfaces;

public interface IEnrollmentRepository
{
    Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct);
    Task AddAsync(Enrollment enrollment, CancellationToken ct);

    Task<IReadOnlyList<Enrollment>> GetByStudentIdAsync(int studentId, CancellationToken ct);
    Task<IReadOnlyList<EnrollmentResponseDto>> GetAllAsync(CancellationToken ct);
    Task<Enrollment?> GetByIdAsync(int id, CancellationToken ct);
    Task UpdateAsync(Enrollment enrollment, CancellationToken ct);

    Task<Enrollment?> GetByStudentAndCourseAsync(
        int studentId, int courseId, CancellationToken ct);

}