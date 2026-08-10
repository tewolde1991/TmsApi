using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers;


[ApiController]
[Route("api/students")]
[Tags("Students")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class StudentController( IStudentService studentService, LinkGenerator linkGenerator): ControllerBase
{
    
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<StudentResponseDto>),StatusCodes.Status200OK)]
    [EndpointSummary("List student with pagination")]
    public async Task<IActionResult> GetStudents([FromQuery] PagedRequest request, CancellationToken ct)
    {
        var result = await studentService.GetStudentsAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}", Name = nameof(GetStudentById))]
    [ProducesResponseType(typeof(StudentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a student by ID")]
    public async Task<IActionResult> GetStudentById(int id, CancellationToken ct)
    {
        var student = await studentService.GetDetailByIdAsync(id, ct);
        if (student is null) return NotFound();

        var links = new List<LinkDto>
        {
            new(linkGenerator.GetPathByName(HttpContext, nameof(GetStudentById), new { id })!, "self", "GET"),
            new(linkGenerator.GetPathByName(HttpContext, nameof(GetStudentById), new { id })!, "update", "PUT"),
            new(linkGenerator.GetPathByName(HttpContext, "ListStudentCertificates", new { studentId = id })!, "certificates", "GET")
        };

        var result = student with { Links = links };
        return Ok(result);
    }


[HttpPost]
    [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Register a new student")]
    public async Task<IActionResult> RegisterStudent(CreateStudentRequest request, CancellationToken ct)
    {
        try
        {
            var result = await studentService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetStudentById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Registration number already exists",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }
}