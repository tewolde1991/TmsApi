

using TmsApi.Application.DTOs;

namespace TmsApi.Infrastructure.Services;

public interface ICertificateService
{
    Task<IReadOnlyList<CertificateResponse>> GetByStudentAsync(int studentId, CancellationToken ct);
    Task<CertificateResponse?> GetByIdAsync(int studentId, int id, CancellationToken ct);
    Task<CertificateResponse> IssueAsync(int studentId, IssueCertificateRequest request, CancellationToken ct);
}