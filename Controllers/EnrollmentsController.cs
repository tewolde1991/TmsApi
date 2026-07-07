using Microsoft.AspNetCore.Mvc;
using TmsApi;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
{
    // private readonly IEnrollmentService _enrollmentService;
    // public EnrollmentsController(IEnrollmentService enrollmentService)
    // {
    //     _enrollmentService  =enrollmentService;
    // }
    // GET/api/enrollments returns all enrollment records

    
   
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var enrollments = await enrollmentService.GetAllAsync(ct);
        return Ok(enrollments);
    }
    // GET/api/enrollments/{id} returns one or 404
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var record = await enrollmentService.GetByIdAsync(id, ct);
        return record is not null ? Ok(record) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request, CancellationToken ct)
    {
        var record = await enrollmentService.EnrollAsync(request.StudentId, request.CourseCode, ct);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id,CancellationToken ct)
    {
        var deleted = await enrollmentService.DeleteAsync(id,ct);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("archive")]
public async Task<IActionResult> ArchiveByYear([FromQuery] int year, CancellationToken ct)
{
    var count = await enrollmentService.ArchiveByYearAsync(year, ct);
    return Ok(new { year, archivedCount = count });
}

}

public record CreateEnrollmentRequest(int StudentId, string CourseCode);
