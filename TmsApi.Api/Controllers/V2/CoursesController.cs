using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Courses.Commands;
using TmsApi.Application.Courses.Queries;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(ISender sender) : ControllerBase
{

    [HttpPost]
    [ProducesResponseType(typeof(CourseCreateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Create a new course")]
    [EndpointDescription("Create a course with a unique code. Returns 409 if the course code already exists")]
    // public async Task<IActionResult> createCourse( [FromBody]
    //     CreateCourseCommands command, CancellationToken ct
    // )
    // {
    //     try
    //     {
    //         var result = await sender.Send(command, ct);
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e);
    //         throw;
    //     } 
    // }
    [HttpGet]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetCoursesQuery(page, pageSize),
            ct);

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCourse(
        int id,
        [FromBody] UpdateCourseCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
        {
            return BadRequest("Route id and command id must match.");
        }

        var updated = await sender.Send(command, ct);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }
    
}