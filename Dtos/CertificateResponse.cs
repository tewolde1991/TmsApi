
using TmsApi.Entities;

namespace Tms.Api.Dtos;

public record CertificateResponse(int Id, string SerialNumber, DateTime IssuedAt, int StudentId, int CourseId)
{
    public CertificateResponse(Certificate certificate)
        : this(certificate.Id, certificate.SerialNumber, certificate.IssuedAt, certificate.StudentId, certificate.CourseId)
    {
    }
}