using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Enrollments.Queries;

public class GetAllEnrollmentsHandler(IEnrollmentRepository enrollmentRepository)
    : IRequestHandler<GetAllEnrollmentsQuery, IReadOnlyList<EnrollmentResponseDto>>
{
    public async Task<IReadOnlyList<EnrollmentResponseDto>> Handle(
        GetAllEnrollmentsQuery request,
        CancellationToken ct)
    {
        return await enrollmentRepository.GetAllAsync(ct);
    }
}