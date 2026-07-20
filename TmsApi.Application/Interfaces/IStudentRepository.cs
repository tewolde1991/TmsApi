

using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id, CancellationToken ct);

    Task<IReadOnlyList<Student>> GetPagedAsync(
        int page,
        int pageSize, CancellationToken ct);

    Task<Student> AddAsync(Student student, CancellationToken ct);

    Task UpdateAsync(Student student, CancellationToken ct);
}