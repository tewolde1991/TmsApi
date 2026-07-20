using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly TmsDbContext _context;

    public EnrollmentRepository(TmsDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(
        int studentId,
        string courseCode,
        CancellationToken ct)
    {
        return await _context.Enrollments
            .Include(e => e.Course)
            .AnyAsync(
                e => e.StudentId == studentId
                     && e.Course.Code == courseCode,
                ct);
    }

    public async Task AddAsync(Enrollment enrollment, CancellationToken ct)
    {
        await _context.Enrollments.AddAsync(enrollment, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Enrollment>> GetByStudentIdAsync(
        int studentId,
        CancellationToken ct)
    {
        return await _context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .OrderBy(e => e.Course.Title)
            .ToListAsync(ct);
    }
}