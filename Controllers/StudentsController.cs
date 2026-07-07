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