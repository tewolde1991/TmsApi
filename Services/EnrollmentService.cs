

using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

public class EnrollmentService
{
    private readonly TmsDbContext _context;
    public EnrollmentService(TmsDbContext context)
    {
        _context = context;
    }
    // archive all enrollments for a give year
    // public async Task<int> ArchiveEnrollmentsByYearAsync(int year, CancellationToken ct = default)
    // {
    //     var affectedRows = await _context.Enrollments.Where(e=>e.Year==year && !e.IsArchived)
    //     .ExecuteUpdateAsync(setters => setters.SetProperty(e =>e.IsArchived, true)
    //     .SetProperty(e=>e.EnrolledAt, e =>e.EnrolledAt), ct);

    //     return affectedRows;
    // }

    public async Task<int> ArchiveEnrollmentsByYearAsync(int year, CancellationToken ct = default)
    {
        var affected = await _context.Enrollments
                            .IgnoreQueryFilters() //Include archived and non-archived
                            .Where(e=>e.Year == year && !e.IsArchived)
                            .ExecuteUpdateAsync(setters =>setters
                            .SetProperty(e=>e.IsArchived, true),
                           ct );

                           return affected;
                            
    }
}