// // // using Microsoft.AspNetCore.Mvc;
// // // public record Course(string Id, string CourseCode, string Title, int Credits);

// // // public record CreateCourseRequest(string CourseCode, string Title, int Credits);
// // // [ApiController]
// // // [Route("api/courses")]
// // // public class CoursesController(ICourseService courseService) : ControllerBase
// // // {
// // //   // GET /api/courses
// // //   [HttpGet]
// // //   public async Task<IActionResult> GetAll()
// // //   {
// // //     var courses = await courseService.GetAllAsync();
// // //     return Ok(courses);
// // //   }

// // //   // GET /api/courses/{id}
// // //   [HttpGet("{id}")]
// // //   public async Task<IActionResult> GetById(string id)
// // //   {
// // //     var course = await courseService.GetByIdAsync(id);
// // //     return course is not null ? Ok(course) : NotFound();
// // //   }

// // //   // POST /api/courses
// // //   [HttpPost]
// // //   public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
// // //   {
// // //     var course = await courseService.CreateAsync(request.CourseCode, request.Title, request.Credits);
// // //     return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
// // //   }

// // //   // DELETE /api/courses/{id}
// // //   [HttpDelete("{id}")]
// // //   public async Task<IActionResult> Delete(string id)
// // //   {
// // //     var deleted = await courseService.DeleteAsync(id);
// // //     return deleted ? NoContent() : NotFound();
// // //   }
// // // }

// // //  module 6

// // using Microsoft.AspNetCore.Mvc;
// // using TmsApi.Entities;
// // using TmsApi.Services;

// // namespace TmsApi.Controllers;

// // [ApiController]
// // [Route("api/courses")]
// // public class CoursesController(ICourseService courseService) : ControllerBase
// // {
// //   [HttpGet("{id:int}", Name = nameof(GetCourseById))]
// //   public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
// //   {
// //     var course = await courseService.GetByIdAsync(id, ct);

// //     if (course is null)
// //       return NotFound();

// //     return Ok(course);
// //   }

// //   [HttpPost]
// //   public async Task<IActionResult> CreateCourse(Course course, CancellationToken ct)
// //   {
// //     var result = await courseService.CreateAsync(course, ct);

// //     return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
// //   }
// // }

// // Module 6
// using Microsoft.AspNetCore.Mvc;
// using TmsApi.Dtos;
// using TmsApi.Services;

// namespace TmsApi.Controllers;

// [ApiController]
// [Route("api/courses")]
// public class CoursesController(ICourseService courseService) : ControllerBase
// {
//   // GET /api/courses/{id}
//   [HttpGet("{id:int}", Name = nameof(GetCourseById))]
//   public async Task<IActionResult> GetCourseById(
//       int id, CancellationToken ct)
//   {
//     // CodeExistsAsync ይህ ቦታ አይደለም ❌
//     var course = await courseService.GetByIdAsync(id, ct);
//     return course is not null ? Ok(course) : NotFound();
//   }

//   // POST /api/courses
//   [HttpPost]
//   public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
//   {
//     // ✅ CodeExistsAsync ይህ ቦታ ነው — CreateCourse ውስጥ
//     if (await courseService.CodeExistsAsync(request.Code, ct))
//       return Conflict(new ProblemDetails
//       {
//         Title = "Course code already exists",
//         Detail = $"A course with code '{request.Code}' is already registered.",
//         Status = StatusCodes.Status409Conflict
//       });

//     var result = await courseService.CreateAsync(request, ct);

//     // ✅ result.Id — 
//     return CreatedAtAction(
//         nameof(GetCourseById),
//         new { id = result.Id },
//         result);
//   }
//   [HttpGet]
//   public async Task<IActionResult> GetCourses(
//   [FromQuery] PagedRequest request, CancellationToken ct)
//   {
//     var result = await courseService.GetCoursesAsync(request, ct);
//     return Ok(result);
//   }
// }

//  Module 6 Session 3 E-5

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

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
    var result = await courseService.GetCoursesAsync(request, ct);
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
      CreateCourseRequest request, CancellationToken ct)
  {
    throw new NotImplementedException();
  }
}