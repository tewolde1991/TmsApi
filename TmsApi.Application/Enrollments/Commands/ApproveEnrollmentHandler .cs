using MediatR;
using TmsApi.Application.Common;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Enums;

namespace TmsApi.Application.Enrollments.Commands;

public class ApproveEnrollmentHandler
    : IRequestHandler<ApproveEnrollmentCommand, Result<Unit, Error>>
{
    private readonly IEnrollmentRepository _repo;

    public ApproveEnrollmentHandler(IEnrollmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<Unit, Error>> Handle(
        ApproveEnrollmentCommand request,
        CancellationToken ct)
    {
        var enrollment = await _repo.GetByIdAsync(request.Id, ct);

        if (enrollment is null)
        {
            return Result<Unit, Error>.Failure(
                /* TODO: replace with your project's not-found Error factory */
                default!);
        }

        if (enrollment.Status != EnrollmentStatus.Pending)
        {
            return Result<Unit, Error>.Failure(
                Error.NotFound("enrollment_not_found", "Enrollment not found."));
               
        }

        if (enrollment.Status != EnrollmentStatus.Pending)
        {
            return Result<Unit, Error>.Failure(
                Error.Conflict("enrollment_not_pending", "Only pending enrollments can be approved."));
        }
        enrollment.Status = EnrollmentStatus.Approved;
        await _repo.UpdateAsync(enrollment, ct);

        return Result<Unit, Error>.Success(Unit.Value);
    }
}