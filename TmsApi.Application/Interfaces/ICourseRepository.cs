using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface  ICourseRepository
{
    Task <Course?> GetByCodeAsync(string courseCode, CancellationToken ct);
    Task<IReadOnlyList<Course>> GetPagedAsync(int page, int pageSize, CancellationToken ct);
    Task<Course> AddAsync(Course course, CancellationToken ct);

    Task<int> CountAsync(CancellationToken ct);
    Task UpdateAsync(Course course, CancellationToken ct);

    Task<Course?> GetByIdAsync(int id, CancellationToken ct); 
    Task<IReadOnlyList<Course>> GetPagedWithEnrollmentsAsync(
        int page,
        int pageSize,
        CancellationToken ct);

    Task<IReadOnlyList<Course>> SearchAsync(
        string? term,
        CancellationToken ct);
}
