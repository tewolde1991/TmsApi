using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Services;
namespace TmsApi.Controllers;


[ApiController]
[Route("api/students")]

public class StudentController: ControllerBase
{
    private readonly StudentService _service;
    public StudentController(StudentService service)
    {
        _service = service;
    }
    [HttpGet("paged")]
    public async Task<IActionResult> GetPagedStudentsAsync(
        int page = 1, 
        int pageSize = 20, 
        CancellationToken ct = default)
    {
        var students = await _service.GetPagedStudentsAsync(page, pageSize, ct);
        return Ok(students);
    }

    [HttpGet("top-courses")]
    public async Task<IActionResult> GetTopCourses(CancellationToken ct = default)
    {
        var topCourses = await _service.GetTop5CoursesAsync(ct);
        return Ok(topCourses);
    }


[HttpPatch("{id}")]
public async Task<IActionResult> PatchStudent(
    int id, 
    string?name, 
    decimal? gpa,
     bool? isActive,
      CancellationToken ct)
    // {
    //     var updated = await _service.UpdateStudentAsync(id, name,gpa,isActive, ct);

    //     if(!updated) return NotFound();

    //     return NoContent();
    // }
    {
        var result = await _service.UpdateStudentWithConcurrencyAsync(
            id,name,gpa,isActive,ct
        );
        return result switch
        {
            UpdateResult.Sucess => NoContent(),
            UpdateResult.NotFound => NotFound(),
            UpdateResult.ConcurrencyConflict => Conflict(new
            {
                message = "The student was modified by another user. Please reload and try again."
            }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}