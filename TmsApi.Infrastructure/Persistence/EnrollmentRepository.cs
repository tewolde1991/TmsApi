using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence;

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
        CancellationToken ct = default)
    {
        return await _context.Enrollments
            .AnyAsync(
                e => e.StudentId == studentId &&
                     e.Course.Code.ToLower() == courseCode.ToLower(),
                ct);
    }

    public async Task AddAsync(
        Enrollment enrollment,
        CancellationToken ct = default)
    {
        await _context.Enrollments.AddAsync(enrollment, ct);
    }

    public async Task SaveChangesAsync(
        CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }

    // Add this method
    public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(
        int studentId,
        CancellationToken ct = default)
    {
        return await _context.Enrollments
            .Where(e => e.StudentId == studentId)
            .AsNoTracking()
            .ToListAsync(ct);
    }
    public async Task<Enrollment?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Enrollments.FindAsync([id], ct);
    }

    public async Task ApproveAsync(int id, CancellationToken ct = default)
    {
        var enrollment = await _context.Enrollments.FindAsync([id], ct);
        if (enrollment is not null)
        {
            enrollment.Status = "Approved";
            await _context.SaveChangesAsync(ct);
        }
    }
    public async Task<IReadOnlyList<EnrollmentResponseDto>> GetAllEnrollmentsAsync(
     CancellationToken ct = default)
    {
        return await _context.Enrollments
            .AsNoTracking()
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.Course.Code,
                e.Course.Title,
                e.StudentId,
                e.Student.FirstName + " " + e.Student.LastName,
                e.IsArchived ? "Archived" : "Active",   // or add a Status field to Enrollment
                e.EnrolledAt
            ))
            .ToListAsync(ct);
    }
}