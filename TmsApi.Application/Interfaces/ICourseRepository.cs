using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface ICourseRepository
{
    Task<Course?> GetByCodeAsync(
        string courseCode,
        CancellationToken ct = default);

    Task<bool> ExistsAsync(
        string courseCode,
        CancellationToken ct = default);

    Task AddAsync(
        Course course,
        CancellationToken ct = default);

    Task SaveChangesAsync(
        CancellationToken ct = default);
}