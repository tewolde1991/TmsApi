// using Microsoft.AspNetCore.Mvc;

// public record Student(string Id, string FirstName, string LastName, string Email);

// public record CreateStudentRequest(string FirstName, string LastName, string Email);
// [ApiController]
// [Route("api/students")]
// public class StudentsController(IStudentService studentService) : ControllerBase
// {
//   // GET /api/students
//   [HttpGet]
//   public async Task<IActionResult> GetAll()
//   {
//     var students = await studentService.GetAllAsync();
//     return Ok(students);
//   }

//   // GET /api/students/{id}
//   [HttpGet("{id}")]
//   public async Task<IActionResult> GetById(string id)
//   {
//     var student = await studentService.GetByIdAsync(id);
//     return student is not null ? Ok(student) : NotFound();
//   }

//   // POST /api/students
//   [HttpPost]
//   public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
//   {
//     var student = await studentService.CreateAsync(request.FirstName, request.LastName, request.Email);
//     return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
//   }

//   // DELETE /api/students/{id}
//   [HttpDelete("{id}")]
//   public async Task<IActionResult> Delete(string id)
//   {
//     var deleted = await studentService.DeleteAsync(id);
//     return deleted ? NoContent() : NotFound();
//   }
// }

// Module 6

using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/students")]
[Tags("Students")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class StudentsController(IStudentService studentService) : ControllerBase
{
  // GET /api/students — paginated list
  [HttpGet]
  [ProducesResponseType(typeof(PagedResponse<StudentResponseDto>), StatusCodes.Status200OK)]
  [EndpointSummary("List students with pagination")]
  [EndpointDescription("Returns a paginated, optionally filtered list of students. PageSize is capped at 50.")]
  public async Task<IActionResult> GetStudents(
      [FromQuery] PagedRequest request, CancellationToken ct)
  {
    var result = await studentService.GetStudentsAsync(request, ct);
    return Ok(result);
  }

  // GET /api/students/{id}
  [HttpGet("{id:int}", Name = nameof(GetStudentById))]
  [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [EndpointSummary("Get a student by ID")]
  public async Task<IActionResult> GetStudentById(int id, CancellationToken ct)
  {
    var student = await studentService.GetByIdAsync(id, ct);
    if (student is null) return NotFound();
    return Ok(student);
  }

  // POST /api/students — create
  [HttpPost]
  [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
  [EndpointSummary("Create a new student")]
  [EndpointDescription("Creates a student with a unique registration number. Returns 409 if it already exists.")]
  public async Task<IActionResult> CreateStudent(
      CreateStudentRequest request, CancellationToken ct)
  {
    if (await studentService.RegistrationNumberExistsAsync(request.RegistrationNumber, ct))
    {
      return Conflict(new ProblemDetails
      {
        Title = "Registration number already exists",
        Status = StatusCodes.Status409Conflict
      });
    }

    var created = await studentService.CreateAsync(request, ct);

    return CreatedAtAction(
        nameof(GetStudentById),
        new { id = created.Id },
        created);
  }

  // PUT /api/students/{id} — update
  [HttpPut("{id:int}")]
  [ProducesResponseType(typeof(StudentResponseDto), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [EndpointSummary("Update an existing student")]
  [EndpointDescription("Updates name, email, GPA, and active status. Returns 404 if the student does not exist.")]
  public async Task<IActionResult> UpdateStudent(
      int id, UpdateStudentRequest request, CancellationToken ct)
  {
    var updated = await studentService.UpdateAsync(id, request, ct);
    if (updated is null) return NotFound();
    return Ok(updated);
  }

  // DELETE /api/students/{id} — soft delete
  [HttpDelete("{id:int}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
  [EndpointSummary("Delete a student")]
  [EndpointDescription("Soft-deletes a student (marks as deleted; record is retained for history).")]
  public async Task<IActionResult> DeleteStudent(int id, CancellationToken ct)
  {
    var deleted = await studentService.DeleteAsync(id, ct);
    if (!deleted) return NotFound();
    return NoContent();
  }
}