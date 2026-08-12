using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Commands;

public class ApproveEnrollmentCommandHandler(IEnrollmentRepository repo)
    : IRequestHandler<ApproveEnrollmentCommand, Result<bool, Error>>
{
  public async Task<Result<bool, Error>> Handle(
      ApproveEnrollmentCommand request,
      CancellationToken ct)
  {
    var enrollment = await repo.GetByIdAsync(request.Id, ct);

    if (enrollment is null)
      return Result<bool, Error>.Failure(
          new Error("enrollment_not_found",
              $"Enrollment {request.Id} not found"));

    await repo.ApproveAsync(request.Id, ct);

    return Result<bool, Error>.Success(true);
  }
}