using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using TmsApi.Data;
using TmsApi.Entities;
namespace TmsApi.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(TmsDbContext context) : ControllerBase
{
    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        Console.WriteLine("\n>>> STEP 1: Building the query object (nodatabase contact)...");
        var query = context.Students.Where(s => s.GPA >= 3.0m);
        Console.WriteLine(">>> STEP 2: Appending a sorting clause...");
        var orderedQuery = query.OrderBy(s => s.Name);
        Console.WriteLine(">>> STEP 3: Materializing query into a C# List...");
        var results = orderedQuery.ToList(); // Execution is triggeredhere
        Console.WriteLine(">>> STEP 4: Materialization finished. List populted.\n");
        return Ok(results);
    }

    private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }
    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {

        Console.WriteLine("\n>>> STEP 1: Running non-translatable query...");
        try
        {
            var students = context.Students
            .Where(s => IsHonorRoll(s.GPA))
            .ToList();
            return Ok(students);
        }
        catch (Exception ex)
{
Console.WriteLine($">>> EXCEPTION CAUGHT: {ex.Message}\n");
return BadRequest(new { Message = ex.Message });
}
    }



[HttpGet("n-plus-one")]
public async Task<IActionResult> TestNPlusOne(CancellationToken cancellationToken)
    {
    var students = await context.Students
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
               
                foreach(var student in students)
        {
            // n+1 one query per student to count enrollments
            var count = await context.Enrollments
                        .AsNoTracking()
                        .CountAsync(e =>e.StudentId ==student.Id, cancellationToken);
                        Console.WriteLine($"{student.Name}: {count} enrollments"); 

        }

                Console.WriteLine(">>> N+1 DEMO: Finished.\n");

        return Ok(new
        {
             Message = "Check the console logs for the SQL statements.",
            StudentCount = students.Count,
            Note = "You should see 1 query for Students, plus N queries for Enrollments."
        });
    }



[HttpGet("nplus1-fux")]
public async Task<IActionResult> NPlusOneFix(CancellationToken cancellationToken)
    {
        var report = await context.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Name,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync(cancellationToken);

            foreach (var r in report)
        {
            Console.WriteLine($"{r.Name}: {r.EnrollmentCount} enrollments");
        }
        return Ok(report);
    }
}