using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Services;

public class CertificateService(TmsDbContext context, ILogger<CertificateService> logger) : ICertificateService
{
    public async Task<IReadOnlyList<CertificateResponse>> GetByStudentAsync(int studentId, CancellationToken ct)
    {
        return await context.Certificate
            .AsNoTracking()
            .Where(c => c.StudentId == studentId)
            .Select(c => new CertificateResponse(c.Id, c.SerialNumber, c.IssuedAt, c.StudentId, c.CourseId))
            .ToListAsync(ct);
    }

    public async Task<CertificateResponse?> GetByIdAsync(int studentId, int id, CancellationToken ct)
    {
        return await context.Certificate
            .AsNoTracking()
            .Where(c => c.Id == id && c.StudentId == studentId)
            .Select(c => new CertificateResponse(c.Id, c.SerialNumber, c.IssuedAt, c.StudentId, c.CourseId))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CertificateResponse> IssueAsync(int studentId, IssueCertificateRequest request, CancellationToken ct)
    {
        var studentExists = await context.Students.AnyAsync(s => s.Id == studentId, ct);
        if (!studentExists)
            throw new InvalidOperationException($"Student with ID {studentId} not found.");

        var courseExists = await context.Courses.AnyAsync(c => c.Id == request.CourseId, ct);
        if (!courseExists)
            throw new InvalidOperationException($"Course with ID {request.CourseId} not found.");

        // Business rule: a student shouldn't get two certificates for the same course
        var alreadyIssued = await context.Certificate
            .AnyAsync(c => c.StudentId == studentId && c.CourseId == request.CourseId, ct);
        if (alreadyIssued)
            throw new InvalidOperationException(
                $"A certificate has already been issued to student {studentId} for course {request.CourseId}.");

        var certificate = new Certificate
        {
            SerialNumber = $"CERT-{Guid.NewGuid():N}"[..16].ToUpperInvariant(), // simple unique serial
            StudentId = studentId,
            CourseId = request.CourseId
            // IssuedAt uses the entity's own default (DateTime.UtcNow)
        };

        context.Certificate.Add(certificate);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Certificate {SerialNumber} issued to student {StudentId} for course {CourseId}",
            certificate.SerialNumber, studentId, request.CourseId);

        return new CertificateResponse(certificate.Id, certificate.SerialNumber, certificate.IssuedAt, certificate.StudentId, certificate.CourseId);
    }

}