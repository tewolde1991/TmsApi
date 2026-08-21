using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Commands;

public class RejectEnrollmentCommandHandler(IEnrollmentRepository repo)
    : IRequestHandler<RejectEnrollmentCommand, Result<bool, Error>>
{
  public async Task<Result<bool, Error>> Handle(
      RejectEnrollmentCommand request,
      CancellationToken ct)
  {
    var enrollment = await repo.GetByIdAsync(request.Id, ct);

    if (enrollment is null)
      return Result<bool, Error>.Failure(
          new Error("enrollment_not_found",
              $"Enrollment {request.Id} not found"));

    await repo.RejectAsync(request.Id, ct);

    return Result<bool, Error>.Success(true);
  }
}