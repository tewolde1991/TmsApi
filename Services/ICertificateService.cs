

using Tms.Api.Dtos;

public interface ICertificateService
{
    Task<IReadOnlyList<CertificateResponse>> GetByStudentAsync(int StudentId, CancellationToken ct);
    Task<CertificateResponse?> GetByIdAsync(int StudentId, int id, CancellationToken ct);
    Task<CertificateResponse> IssueAsync(int StudentId, IssueCertificateRequest request, CancellationToken ct);
}