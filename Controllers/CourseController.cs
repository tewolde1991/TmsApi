using Microsoft.AspNetCore.Mvc;
using TmsApi;
[ApiController]
[Route("/api/courses")]


public class CourseController(ICourseService courseService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
       var courses = await courseService.GetAllAsync();
       return Ok(courses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var record = await courseService.GetByIdAsync(id);
        return record is not null ? Ok(record): NotFound();

    }
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
    {
        var record = await courseService.CourseAsync(request.CourseCode, request.Capacity);
        return CreatedAtAction(nameof(GetById), new {id = record.Code}, record);
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await courseService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

public record CreateCourseRequest(string CourseCode, int Capacity)
{
    // internal string id;
}