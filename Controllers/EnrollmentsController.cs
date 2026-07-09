using Microsoft.AspNetCore.Mvc;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
public class EnrollmentsController(
    ICourseService courseService,
    IEnrollmentService enrollmentService) : ControllerBase
{
    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    public async Task<IActionResult> GetEnrollment(int courseId, int id, CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetByIdAsync(courseId, id, ct);
        return enrollment is not null ? Ok(enrollment) : NotFound();
    }

    [HttpPost]
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