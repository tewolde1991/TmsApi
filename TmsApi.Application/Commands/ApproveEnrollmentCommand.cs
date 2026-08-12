using MediatR;
using TmsApi.Application.Commands;
using TmsApi.Application.Dtos;
using TmsApi.Application.Queries;
using TmsApi.Application.Common;

namespace TmsApi.Application.Commands;

public record ApproveEnrollmentCommand(int Id) : IRequest<Result<bool, Error>>;
