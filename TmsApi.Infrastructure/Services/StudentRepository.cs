using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class StudentRepository : IStudentRepository
{
    private readonly TmsDbContext _context;

    public StudentRepository(TmsDbContext context)
    {
        _context = context;
    }

    public async Task<Student?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.Students
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public  async Task<IReadOnlyList<Student>> GetPagedAsync(int page, int pageSize, CancellationToken ct)
    {
        return await _context.Students
            .OrderBy(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<Student> AddAsync(Student student, CancellationToken ct)
    {
        await _context.Students.AddAsync(student, ct);
        await _context.SaveChangesAsync(ct);
        return student;

    }

    public async Task UpdateAsync(Student student, CancellationToken ct)
    {
        _context.Students.Update(student);
        await _context.SaveChangesAsync(ct);
    }
}