using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TmsApi.Api.Hubs;
using TmsApi.Application.Commands;
using TmsApi.Application.Dtos;


// using TmsApi.Application.DTOs;
// using TmsApi.Application.Enrollments.Commands;
// using TmsApi.Application.Enrollments.Queries;
using TmsApi.Application.Hubs;
using TmsApi.Application.Queries;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
public class EnrollmentsController(
// ICourseService courseService,
// IEnrollmentService enrollmentService,
IMediator mediator, IHubContext<TmsHub, ITmsHubClient> hubContext) : ControllerBase
{
  [HttpPost]
  public async Task<IActionResult> Enroll(EnrollStudentCommand command, CancellationToken ct)
  {
    var result = await mediator.Send(command, ct);

    return result.Match(
        onSuccess: created => CreatedAtAction(nameof(GetSchedule),
            new { studentId = created.StudentId }, created),
        onFailure: error =>
        {
          var status = error.Code switch
          {
            "course_not_found" => StatusCodes.Status404NotFound,
            "course_full" or "already_enrolled" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
          };

          return Problem(
                  statusCode: status,
                  title: "Enrollment rejected",
                  detail: error.Message,
                  type: $"https://tms.local/errors{error.Code}");
        });
  }

  [HttpGet("{studentId}/schedule")]

  public async Task<IActionResult> GetSchedule(
      int studentId,
      CancellationToken ct)
  {
    var schedule = await mediator.Send(
        new GetStudentScheduleQuery(studentId),
        ct);

    return Ok(schedule);
  }

  [HttpGet]
  [ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponseDto>), StatusCodes.Status200OK)]
  [EndpointSummary("Get all enrollments")]
  public async Task<IActionResult> GetAll(CancellationToken ct)
  {
    var enrollments = await mediator.Send(new GetAllEnrollmentsQuery(), ct);
    return Ok(enrollments);
  }

  [HttpPost("{id}/approve")]
  public async Task<IActionResult> Approve(int id, CancellationToken ct)
  {
    var result = await mediator.Send(
        new ApproveEnrollmentCommand(id),
        ct);

    return await result.Match<Task<IActionResult>>(
        onSuccess: async _ =>
        {
          // Database update succeeded.
          // Now notify all connected Angular clients.
          await hubContext.Clients.All
          .RecieveEnrollmentStatusUpdated(   // ← fix spelling
        id.ToString(),
        "Approved");

          return NoContent();
        },
        onFailure: error =>
        {
          var status = error.Code switch
          {
            "enrollment_not_found" =>
                    StatusCodes.Status404NotFound,

            "enrollment_not_pending" =>
                    StatusCodes.Status409Conflict,

            _ =>
                    StatusCodes.Status400BadRequest
          };

          return Task.FromResult<IActionResult>(
                  Problem(
                      statusCode: status,
                      title: "Approval rejected",
                      detail: error.Message,
                      type: $"https://tms.local/errors/{error.Code}"
                  )
              );
        });
  }
  [HttpPost("{id}/reject")]
  public async Task<IActionResult> Reject(int id, CancellationToken ct)
  {
    var result = await mediator.Send(new RejectEnrollmentCommand(id), ct);

    return await result.Match<Task<IActionResult>>(
        onSuccess: async _ =>
        {
          // ← Broadcast to ALL connected Angular clients
          await hubContext.Clients.All
                  .RecieveEnrollmentStatusUpdated(
                      id.ToString(),
                      "Rejected");
          return NoContent();
        },
onFailure: error => Task.FromResult<IActionResult>(
                Problem(
                    statusCode: 404,
                    title: "Rejection failed",
                    detail: error.Message
                )
            )
        );
  }

}
