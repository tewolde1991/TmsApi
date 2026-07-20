
using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Routing;
using TmsApi.Application.Dtos;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]   // Exercise 6 — Scalar grouping
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CoursesController(
    ICourseService courseService,
    LinkGenerator linkGenerator)   // ← inject LinkGenerator
    : ControllerBase
{
  // GET /api/courses — paginated list (unchanged)
  [HttpGet]
  [ProducesResponseType(typeof(PagedResponse<CourseResponseDto>), StatusCodes.Status200OK)]
  [EndpointSummary("Get paginated list of courses")]
  [EndpointDescription("Returns a paged list of courses with optional search and ordering.")]
  public async Task<IActionResult> GetCourses(
      [FromQuery] PagedRequest request, CancellationToken ct)
  {
    var result = await courseService.GetCoursesAsync(request, ct).ConfigureAwait(false);
    return Ok(result);
  }

  // GET /api/courses/{id} — detail with HATEOAS links
  [HttpGet("{id:int}", Name = nameof(GetCourseById))]
  [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [EndpointSummary("Get a course by ID")]
  [EndpointDescription("Returns course details with HATEOAS links. Returns 404 if the course does not exist.")]
  public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
  {
    var course = await courseService.GetByIdAsync(id, ct);
    if (course is null) return NotFound();

    // TODO 1 + TODO 2: Build links
    var links = new List<LinkDto>
    {
        new(
            linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id })!,
            "self",
            "GET"),
        new(
            linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id })!,
            "update",
            "PUT"),
        new(
            linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new { id })!,
            "delete",
            "DELETE"),
    };

    var enrollmentsHref = linkGenerator.GetPathByName(
        HttpContext, "ListCourseEnrollments", new { courseId = id })!;

    links.Add(new LinkDto(enrollmentsHref, "enrollments", "GET"));

    if (course.EnrollmentCount < course.MaxCapacity)
    {
      links.Add(new LinkDto(enrollmentsHref, "enroll", "POST"));
    }

    // TODO 3: Build the detail DTO
    var detailDto = new CourseDetailDto
    {
      Id = course.Id,
      Code = course.Code,
      Title = course.Title,
      MaxCapacity = course.MaxCapacity,
      EnrollmentCount = course.EnrollmentCount,
      Links = links
    };

    return Ok(detailDto);
  }

  // POST /api/courses — create new course

  [HttpPost]
  [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
  [EndpointSummary("Create a new course")]
  [EndpointDescription("Creates a course with a unique code. Returns 409 if the course code already exists.")]
  public async Task<IActionResult> CreateCourse(
      CreateCourseRequest request, ICourseService courseService1, CancellationToken ct)
  {
    var course = await courseService1.CreateAsync(request, ct);
    return CreatedAtAction(nameof(GetCourseById), new { id = course.Id }, course);
  }

}