using TmsApi.Application.Dtos;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface IEnrollmentRepository
{
    Task<bool> ExistsAsync(
        int studentId,
        string courseCode,
        CancellationToken ct = default);

    Task AddAsync(
        Enrollment enrollment,
        CancellationToken ct = default);

    Task SaveChangesAsync(
        CancellationToken ct = default);

    // Add this method
    Task<IEnumerable<Enrollment>> GetByStudentIdAsync(
        int studentId,
        CancellationToken ct = default);
    Task<IReadOnlyList<EnrollmentResponseDto>> GetAllEnrollmentsAsync(
        CancellationToken ct = default);

    Task<Enrollment?> GetByIdAsync(int id, CancellationToken ct = default);
    Task ApproveAsync(int id, CancellationToken ct = default);
    Task RejectAsync(
            int id,
            CancellationToken ct = default);
}