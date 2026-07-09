

using Microsoft.AspNetCore.Mvc;

using TmsApi.Services;

[ApiController]
[Route("api/courses")]
public class CourseController(ICourseService courseService
): ControllerBase
{
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
      var course = await courseService.GetByIdAsync(id, ct);
      return course is not null ? Ok(course) : NotFound();
    //   throw new NotImplementedException();  

    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
    {
        try
        {
           var result = await courseService.CreateAsync(request,ct);
           return CreatedAtAction(nameof(GetCourseById), new{id=result.Id},result);
        }
        catch (InvalidOperationException ex) when(ex.Message.Contains("already exist"))
        {
            return Conflict(new ProblemDetails
            {
                Title="Course code already exista",
                Status=StatusCodes.Status409Conflict,
                Detail=ex.Message
                
            });
            
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Course Error:{ex.Message}");
           Console.WriteLine($"Inner: {ex.InnerException?.Message}"); 
           Console.WriteLine(ex.StackTrace);

            return StatusCode(409, new ProblemDetails
            {
                Title="Course code already exista",
                Detail= ex.InnerException?.Message?? 
                ex.Message
            });
        }
       
    }
}