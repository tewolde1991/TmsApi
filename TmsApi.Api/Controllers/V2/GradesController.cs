using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Grades.Commands;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/grades")]
[ApiVersion("2.0")]
[Produces("application/json")]
public class GradesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(
        typeof(object),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitGrade(
        [FromBody] SubmitGradeCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        return Ok(new
        {
            id = result.Id.ToString(),
            success = true
        });
    }
}