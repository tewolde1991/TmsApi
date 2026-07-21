using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Courses.Queries;


namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController(ISender sender) : ControllerBase
{
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
}

