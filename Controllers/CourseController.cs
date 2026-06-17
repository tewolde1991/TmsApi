using Microsoft.AspNetCore.Mvc;
public record Course(string Id, string CourseCode, string Title, int Credits);

public record CreateCourseRequest(string CourseCode, string Title, int Credits);
[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService courseService) : ControllerBase
{
  // GET /api/courses
  [HttpGet]
  public async Task<IActionResult> GetAll()
  {
    var courses = await courseService.GetAllAsync();
    return Ok(courses);
  }

  // GET /api/courses/{id}
  [HttpGet("{id}")]
  public async Task<IActionResult> GetById(string id)
  {
    var course = await courseService.GetByIdAsync(id);
    return course is not null ? Ok(course) : NotFound();
  }

  // POST /api/courses
  [HttpPost]
  public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
  {
    var course = await courseService.CreateAsync(request.CourseCode, request.Title, request.Credits);
    return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
  }

  // DELETE /api/courses/{id}
  [HttpDelete("{id}")]
  public async Task<IActionResult> Delete(string id)
  {
    var deleted = await courseService.DeleteAsync(id);
    return deleted ? NoContent() : NotFound();
  }
}