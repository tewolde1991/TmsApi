using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(
    ICourseService courseService,
    IEnrollmentService enrollmentService) : ControllerBase
{

    [HttpGet(Name ="ListCourseEnrollments")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponseDto>),StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
    [EndpointSummary("List enrollment for a course")]
     public async Task<IActionResult> GetEnrollments(int courseId,CancellationToken ct)
    {
        var courses = await courseService.GetByIdAsync(courseId, ct);
        if (courses is  null) return NotFound();

        var enrollments = await enrollmentService.GetByCourseAsync(courseId, ct);
         return Ok(enrollments);
    }

    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get one enrollment for a course")]
    public async Task<IActionResult> GetEnrollment(int courseId, int id, CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetByIdAsync(courseId, id, ct);
        return enrollment is not null ? Ok(enrollment) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status201Created)]
    
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Enroll a student in acourse")]
    [EndpointDescription("Returns 404 if the course does not exist, 409 if the course has reached MaxCapcity.")]

     public async Task<IActionResult> EnrollStudent(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        // Example: use courseService to validate the course exists before delegating
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Course not found",
                Detail = $"Course with ID {courseId} not found.",
                Status = StatusCodes.Status404NotFound
            });
        }

        try
        {
            var result = await enrollmentService.CreateAsync(courseId, request, ct);
            return CreatedAtAction(
                nameof(GetEnrollment),
                new { courseId, id = result.Id },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Business Rule Violation",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
        catch( Exception ex)
        {
            Console.WriteLine($"Enrollment error:{ex.Message}");
            Console.WriteLine($"Inner: {ex.InnerException?.Message}");
            Console.WriteLine(ex.StackTrace);

            return StatusCode(500, new ProblemDetails
            {
                Title="Internal server error",
                Detail= ex.InnerException?.Message?? 
                ex.Message
            });
        }
    }
}