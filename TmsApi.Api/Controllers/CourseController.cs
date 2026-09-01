using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Infrastructure.Services;
using TmsApi.Application.Interfaces;
namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/courses")]
[Tags("Courses")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CourseController(ICourseService courseService, LinkGenerator linkGenerator
): ControllerBase
{

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CourseResponseDto>),StatusCodes.Status200OK)]
    [EndpointSummary("List courses with pagination")]
    [EndpointDescription("Returns a paginated, optionally filtered list of TMS courses. PageSise is capped at 50.")]
    public async Task<IActionResult> GetCourses(
        [FromQuery] PagedRequest request, CancellationToken ct
    )
    {
        var result = await courseService.GetCoursesAsync(request, ct);
        return Ok(result);
    }
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    [ProducesResponseType(typeof(CourseDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a course by ID")]
    [EndpointDescription("Returns course details with HATEOAS links.Return 404 if the course does not exists.")]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        if( course is  null ) return NotFound();

        var links = new List<LinkDto>
        {
            new(
                Href:  linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new{id})!,
                Rel: "self",
                Method:"GET"

            ),
            new(
                Href: linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new {id})!,
                Rel: "update",
                Method: "PUT"
            ),
            new(
                Href: linkGenerator.GetPathByName(HttpContext, nameof(GetCourseById), new {id})!,
                Rel:"delete",
                Method: "DELETE"
            ),
            new(
                Href: linkGenerator.GetPathByName(HttpContext,"ListCourseEnrollments", new {courseId = id})!,
                Rel:"enrollments",
                Method: "GET"
            ),
        };

        if(course.EnrollmentCount < course.MaxCapacity)
        {
            links.Add(new LinkDto(
                Href:linkGenerator.GetPathByName(HttpContext,"ListCourseEnrollments", new{courseId = id})!,
                Rel:"enroll",
                Method: "POST"
            ));
        }

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


    [HttpPost]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Create a new course")]
    [EndpointDescription("Create a course with a unique code. returns409 if the course code already exist")]
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