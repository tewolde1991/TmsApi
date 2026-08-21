using MediatR;
using TmsApi.Application.Common;

namespace TmsApi.Application.Commands;

public record RejectEnrollmentCommand(int Id) : IRequest<Result<bool, Error>>;