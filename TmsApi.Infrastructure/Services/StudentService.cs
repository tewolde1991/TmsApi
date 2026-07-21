
// module 6
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Dtos;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Services;

public class StudentService(TmsDbContext context) : IStudentService
{
    private static StudentResponseDto MapToDto(Student student) =>
        new(
            student.Id,
            student.RegistrationNumber,
            student.FirstName,
            student.LastName,
            student.Email,
            student.GPA,
            student.IsActive,
            student.Enrollments.Count,
            student.Enrollments
                .Where(e => e.Course != null)
                .Select(e => new StudentCourseDto(
                    e.CourseId,
                    e.Course.Code,
                    e.Course.Title))
                .ToList());

    // ---------- READ (single) ----------
    public async Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var student = await context.Students
            .AsNoTracking()
            .Where(s => s.Id == id && !s.IsDeleted)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(ct);

        return student is null ? null : MapToDto(student);
    }

    // ---------- READ (list, paginated) ----------
    public async Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(
        PagedRequest request, CancellationToken ct)
    {
        IQueryable<Student> query = context.Students
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(s =>
                EF.Functions.ILike(s.FirstName, $"%{request.Search}%") ||
                EF.Functions.ILike(s.LastName, $"%{request.Search}%") ||
                EF.Functions.ILike(s.Email, $"%{request.Search}%") ||
                EF.Functions.ILike(s.RegistrationNumber, $"%{request.Search}%"));
        }

        var totalCount = await query.CountAsync(ct);

        IQueryable<Student> sortedQuery = request.OrderBy switch
        {
            "LastName" => request.Descending
                ? query.OrderByDescending(s => s.LastName)
                : query.OrderBy(s => s.LastName),
            "GPA" => request.Descending
                ? query.OrderByDescending(s => s.GPA)
                : query.OrderBy(s => s.GPA),
            "RegistrationNumber" => request.Descending
                ? query.OrderByDescending(s => s.RegistrationNumber)
                : query.OrderBy(s => s.RegistrationNumber),
            _ => request.Descending
                ? query.OrderByDescending(s => s.FirstName)
                : query.OrderBy(s => s.FirstName),
        };

        var items = await sortedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var mappedItems = items.Select(MapToDto).ToList();

        return new PagedResponse<StudentResponseDto>
        {
            Items = mappedItems,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    // ---------- Uniqueness check ----------
    public Task<bool> RegistrationNumberExistsAsync(string registrationNumber, CancellationToken ct) =>
        context.Students.AnyAsync(s => s.RegistrationNumber == registrationNumber && !s.IsDeleted, ct);

    // ---------- CREATE ----------
    public async Task<StudentResponseDto> CreateAsync(
        CreateStudentRequest request, CancellationToken ct)
    {
        var student = new Student
        {
            RegistrationNumber = request.RegistrationNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            GPA = request.GPA,
            IsActive = true,
            IsDeleted = false
        };

        context.Students.Add(student);
        await context.SaveChangesAsync(ct);

        return new StudentResponseDto(
            student.Id, student.RegistrationNumber, student.FirstName,
            student.LastName, student.Email, student.GPA, student.IsActive, 0,
            Array.Empty<StudentCourseDto>());
    }

    // ---------- UPDATE ----------
    public async Task<StudentResponseDto?> UpdateAsync(
        int id, UpdateStudentRequest request, CancellationToken ct)
    {
        var student = await context.Students
            .Where(s => s.Id == id && !s.IsDeleted)
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(ct);

        if (student is null) return null;

        student.FirstName = request.FirstName;
        student.LastName = request.LastName;
        student.Email = request.Email;
        student.GPA = request.GPA;
        student.IsActive = request.IsActive;

        await context.SaveChangesAsync(ct);

        return MapToDto(student);
    }

    // ---------- DELETE (soft delete) ----------
    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var student = await context.Students
            .Where(s => s.Id == id && !s.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (student is null) return false;

        student.IsDeleted = true;
        student.IsActive = false;
        await context.SaveChangesAsync(ct);

        return true;
    }
}