
// using Microsoft.AspNetCore.Mvc;
// using TmsApi;
// using TmsApi.Models;
// [ApiController]
// [Route("/api/students")]
// public class StudentController(IStudentService studentService) : ControllerAttribute
// {
//     [HttpGet]
//     public async Task<IActionResult> GetAll()
//     {
//         var student = await studentService.GetAllAsync();
//         return Ok(student);
        
//     }

//     private IActionResult Ok(IReadOnlyList<StudentRecord> student)
//     {
//         throw new NotImplementedException();
//     }

//     [HttpGet("{id}")]

//     public async Task<IActionResult> GetById(string id)
//     {
//         var record = await studentService.GetByIdAsync(id);
//         return record is not null ? Ok(record) : NotFound();
//     }
//     [HttpPost]
//      public async Task<IActionResult> Create([FromBody] CreateCourseRequest request)
//     {
//         var record = await studentService.StudentAsync(request.id, request.ame, request.Age,request.GPA);
//         return CreatedAtAction(nameof(GetById), new {id = record.Name}, record);
//     }

//     private IActionResult CreatedAtAction(string v, object value, object record)
//     {
//         throw new NotImplementedException();
//     }

//     private IActionResult NotFound()
//     {
//         throw new NotImplementedException();
//     }

//     private IActionResult Ok(StudentRecord record)
//     {
//         throw new NotImplementedException();
//     }
// }
// public record CreateCourseRequest(string id, string Name, int Age, decimal GPA)
// {
    
// }