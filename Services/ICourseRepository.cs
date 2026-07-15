

using TmsApi.Entities;

public interface  ICourseRepository
{
    Task <Course?> GetByCodeAsync(string courseCode, CancellationToken ct);

}
public interface IEnrollmentRepository
{
    Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct);
    Task AddAsync(Enrollment enrollment, CancellationToken ct);

    Task<IReadOnlyList<Enrollment>> GetByStudentIdAsync(int studentId, CancellationToken ct);
}