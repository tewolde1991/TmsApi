using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]   // Exercise 6 — Scalar grouping
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(
    ICourseService courseService,
    IEnrollmentService enrollmentService) : ControllerBase
{
  // GET /api/courses/{courseId}/enrollments — list
  [HttpGet(Name = "ListCourseEnrollments")]
  [ProducesResponseType(typeof(List<EnrollmentResponseDto>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [EndpointSummary("List enrolments for a course")]
  public async Task<IActionResult> GetEnrollments(
      int courseId, CancellationToken ct)
  {
    var course = await courseService.GetByIdAsync(courseId, ct);
    if (course is null) return NotFound();

    var enrollments = await enrollmentService
        .GetByCourseAsync(courseId, ct);
    return Ok(enrollments);
  }

  // GET /api/courses/{courseId}/enrollments/{id} — single
  [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
  [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [EndpointSummary("Get one enrolment for a course")]
  public async Task<IActionResult> GetEnrollment(
      int courseId, int id, CancellationToken ct)
  {
    var e = await enrollmentService.GetByIdAsync(courseId, id, ct);
    return e is not null ? Ok(e) : NotFound();
  }

  // POST /api/courses/{courseId}/enrollments — enroll
  [HttpPost]
  [ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
  [EndpointSummary("Enrol a student in a course")]
  [EndpointDescription("Returns 404 if the course does not exist, 409 if the course has reached MaxCapacity.")]
  public async Task<IActionResult> EnrollmentStudent(
      int courseId, EnrollmentStudentRequest request, CancellationToken ct)
  {
    var course = await courseService.GetByIdAsync(courseId, ct);
    if (course is null) return NotFound();

    if (course.EnrollmentCount >= course.MaxCapacity)
      return Conflict(new ProblemDetails
      {
        Title = "Course is full",
        Detail = $"Course '{course.Title}' has reached its maximum capacity of {course.MaxCapacity}.",
        Status = StatusCodes.Status409Conflict
      });

    var enrollment = await enrollmentService
        .CreateAsync(courseId, request, ct);
    return CreatedAtAction(
        nameof(GetEnrollment),
        new { courseId, id = enrollment.Id },
        enrollment);
  }
}
