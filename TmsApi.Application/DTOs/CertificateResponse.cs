
using TmsApi.Domain.Entities;

namespace TmsApi.Application.DTOs;

public record CertificateResponse(int Id, string SerialNumber, DateTime IssuedAt, int StudentId, int CourseId)
{
    public CertificateResponse(Certificate certificate)
        : this(certificate.Id, certificate.SerialNumber, certificate.IssuedAt, certificate.StudentId, certificate.CourseId)
    {
    }
}