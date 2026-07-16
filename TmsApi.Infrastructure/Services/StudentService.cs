using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class StudentService(TmsDbContext context, ILogger<StudentService> logger): IStudentService
{
    public async Task<StudentResponseDto?> GetByIdAsync(
        int id,  CancellationToken ct )
    {
   return await context.Students
                        .AsNoTracking()
                        .Where(s=>s.Id == id)
                        .Select(s=> new StudentResponseDto(s.Id,s.RegistrationNumber,s.Name,s.GPA,s.IsActive))
                        .FirstOrDefaultAsync(ct);
        
    }

    public async Task<StudentDetailDto?> GetDetailByIdAsync(int id, CancellationToken ct)
    {
        return await context.Students
                    .AsNoTracking()
                    .Where(s=>s.Id == id)
                    .Select(s=> new StudentDetailDto
                    {
                        Id = s.Id,
                        RegistrationNumber = s.RegistrationNumber,
                        Name = s.Name,
                        GPA = s.GPA,
                        IsActive = s.IsActive,
                        EnrollmentCount = s.Enrollments.Count,
                        Links = new List<LinkDto>()
                    })
                    .FirstOrDefaultAsync(ct); 
    }


public async Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(PagedRequest request, CancellationToken ct)
    {
        IQueryable<Student> query = context.Students.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(s=>EF.Functions.ILike(s.Name,$"%{request.Search}%") ||
            EF.Functions.ILike(s.RegistrationNumber, $"%{request.Search}%")); 
            
        }
        var totalCount = await query.CountAsync(ct);

        IQueryable<Student> sortedQuery = request.OrderBy switch
        {
            "RegistrationNumber" => request.Descending
                ? query.OrderByDescending(s=>s.RegistrationNumber)
                :query.OrderBy(s=>s.RegistrationNumber),
            "GPA" => request.Descending
                ?query.OrderByDescending(s=>s.GPA)
                :query.OrderBy(s=>s.GPA),
            _=> request.Descending
                ?query.OrderByDescending(s=>s.Name)
                :query.OrderBy(s=>s.Name)
        };
        var items = await sortedQuery
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(s => new StudentResponseDto(s.Id,s.RegistrationNumber,s.Name,s.GPA,s.IsActive))
                    .ToListAsync(ct);

            return new PagedResponse<StudentResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
    }
    

    public async Task<bool> RegistrationNumberExistsAsync(string registrationNumber, CancellationToken ct)
    {
        return await context.Students.AnyAsync(s=>s.RegistrationNumber == registrationNumber, ct);
    }

    public async Task<StudentResponseDto> CreateAsync(CreateStudentRequest request, CancellationToken ct)
    {
        if(await RegistrationNumberExistsAsync(request.RegistrationNumber, ct))
        throw new InvalidOperationException($"Registration number '{request.RegistrationNumber}' already exists.");
        var student = new Student
        {
            RegistrationNumber = request.RegistrationNumber,
            Name = request.Name,
            GPA = 0,
            IsActive = true
        };

        context.Students.Add(student);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Student {RegistrationNumber} registered with Id {StudentId}",student.RegistrationNumber, student.Id);

        return new StudentResponseDto(student.Id, student.RegistrationNumber, student.Name, student.GPA,student.IsActive);
    }



    // new update student with shadow LastUpdated property
    public async Task<StudentResponseDto> UpdateAsync(
        int id,
       UpdateStudentRequest request,
        CancellationToken ct)
    {
        var student = await context.Students.FirstOrDefaultAsync(s =>s.Id == id, ct);
        if (student is null)
           throw new InvalidOperationException($"Student with ID {id} not found.");
        context.Entry(student).Property(s=>s.Version).OriginalValue = request.Version;

        student.Name = request.Name;
        student.GPA = request.GPA;
        student.IsActive = request.IsActive;

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException("This student record was modified by someone else. Refresh and try again.");
        }

        return new StudentResponseDto(student.Id, student.RegistrationNumber, student.Name, student.GPA,student.IsActive);
    }



    public Task<PagedResponse<StudentResponseDto>> GetStudentAsync(int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}

   