using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Dtos;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers.V2;  

[ApiController]
[Route("api/v2/enrollments")]
[ApiVersion("2.0")]
[Tags("Enrollments")]
public class EnrollmentsController(
    ICourseService     courseService,
    IEnrollmentService enrollmentService) : ControllerBase
{
    // GET /api/v2/courses/{courseId}/enrollments
    [HttpGet]
    [EndpointSummary("Get all enrollments for a course")]
    [ProducesResponseType(typeof(List<EnrollmentResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetEnrollments(
        int courseId, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null) return NotFound();

        var enrollments = await enrollmentService
            .GetByCourseAsync(courseId, ct);
        return Ok(enrollments);
    }

    // GET /api/v2/courses/{courseId}/enrollments/{id}
    [HttpGet("{id:int}", Name = "GetEnrollmentV2")]
    [EndpointSummary("Get a single enrollment by ID")]
    [ProducesResponseType(typeof(EnrollmentResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetEnrollment(
        int courseId, int id, CancellationToken ct)
    {
        var e = await enrollmentService.GetByIdAsync(courseId, id, ct);
        return e is not null ? Ok(e) : NotFound();
    }

    // POST /api/v2/courses/{courseId}/enrollments
    [HttpPost]
    [EndpointSummary("Enroll a student into a course")]
    [ProducesResponseType(typeof(EnrollmentResponseDto), 201)]
    [ProducesResponseType(404)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> EnrollStudent(
        int courseId, EnrollmentStudentRequest request,
        CancellationToken ct)
    {
        // ① 404 — course iyለ?
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null) return NotFound();

        // ② 409 — course iyሞላ?
        if (course.EnrollmentCount >= course.MaxCapacity)
            return Conflict(new ProblemDetails
            {
                Title  = "Course is full",
                Detail = $"Course '{course.Title}' has reached its maximum capacity of {course.MaxCapacity}.",
                Status = StatusCodes.Status409Conflict
            });

        // ③ 201 — enroll
        var enrollment = await enrollmentService
            .CreateAsync(courseId, request, ct);
        return CreatedAtAction(
            nameof(GetEnrollment),
            new { courseId, id = enrollment.Id },
            enrollment);
    }
}