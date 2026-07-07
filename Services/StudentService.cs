using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;

namespace TmsApi.Services;

public class StudentService
{
    private readonly TmsDbContext _context;

    public StudentService(TmsDbContext context)
    {
        _context = context;
    }
    public async Task<List<Student>> GetPagedStudentsAsync(
        int pageNumber, 
        int pageSize = 20, 
        CancellationToken ct = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var students = await _context.Students
            .OrderBy(s => s.Name)    
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return students;
    }

    
    public async Task<List<object>> GetTop5CoursesAsync(CancellationToken ct = default)
    {
        var topCourses = await _context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new 
            {
                CourseTitle = g.Key,
                EnrollmentCount = g.Count()
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .Take(5)
            .ToListAsync(ct);

        return topCourses.Cast<object>().ToList();   // Return as List<object> for simplicity
    }

    // new update student with shadow LastUpdated property
    public async Task<bool> UpdateStudentAsync(
        int id,
        string? newName = null,
        decimal? newGPA = null,
        bool? newIsActive = null,
        CancellationToken ct = default)
    {
        // load student by primary key
        var student = await _context.Students.FindAsync(new object[] {id}, ct);
        if (student is null)
        {
            return false; // Student not found
        }
        if (newName is not null)
        {
            student.Name = newName;
        }
        if (newGPA is not null)
        {
            student.GPA = newGPA.Value;
        }
        if (newIsActive is not null)
        {
            student.IsActive = newIsActive.Value;
        }
        // set  shadow property lastupdated
        _context.Entry(student)
            .Property("LastUpdated")
            .CurrentValue = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return true;
    }

    // existing update method w/o shadow property and concurency handling

    public  async Task<UpdateResult>UpdateStudentWithConcurrencyAsync(
        int id,
        string? newName = null,
        decimal? newGpa = null,
        bool? newIsActive = null,
        CancellationToken ct = default
    )
    {
        var student = await _context.Students.FindAsync(new object[]{id}, ct);
        if (student is null)
        {
            return UpdateResult.NotFound;
        }
        // update properties conditionally
        if (newName is not null)
        {
            student.Name = newName;
        }
        if(newGpa is not null)
        {
            student.GPA = newGpa.Value;
        }
        if (newIsActive is not null)
        {
            student.IsActive = newIsActive.Value;
        }

        // shadow last update
        _context.Entry(student)
                .Property("LastUpdated")
                .CurrentValue = DateTime.UtcNow;

                try
                {
                    await _context.SaveChangesAsync(ct);
                    return UpdateResult.Sucess;
                }
                catch (DbUpdateConcurrencyException)
        {
            // another user updated the same student after we loaded it.
            // version/xmin changed so our update is rejected
            return UpdateResult.ConcurrencyConflict;
        }
                
    }
        
}

public enum UpdateResult
{
    Sucess,
    NotFound,
    ConcurrencyConflict
}