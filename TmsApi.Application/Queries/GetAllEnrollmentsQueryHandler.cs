using MediatR;
using TmsApi.Application.Dtos;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Queries;

public class GetAllEnrollmentsQueryHandler(IEnrollmentRepository repo)
    : IRequestHandler<GetAllEnrollmentsQuery, IReadOnlyList<EnrollmentResponseDto>>
{
    public async Task<IReadOnlyList<EnrollmentResponseDto>> Handle(
        GetAllEnrollmentsQuery request,
        CancellationToken ct)
    {
        return await repo.GetAllEnrollmentsAsync(ct);
    }
}