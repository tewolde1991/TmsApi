using MediatR;
using TmsApi.Application.Dtos;

namespace TmsApi.Application.Queries;

public record GetAllEnrollmentsQuery : IRequest<IReadOnlyList<EnrollmentResponseDto>>;