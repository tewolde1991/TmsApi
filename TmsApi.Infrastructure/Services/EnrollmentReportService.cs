using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class EnrollmentReportService
{
  private readonly TmsDbContext _db;

  public EnrollmentReportService(TmsDbContext db) => _db = db;

  public async Task ShowNPlusOneAsync(CancellationToken ct = default)
  {
    Console.WriteLine("=== PART A: N+1 (BAD) ===");


    var students = await _db.Students
        .AsNoTracking()
        .ToListAsync(ct);

    foreach (var s in students)
    {
      // TODO: uses StudentId get enrollment 
      var count = await _db.Enrollments
          .AsNoTracking()
          .CountAsync(e => e.StudentId == s.Id, ct);

      Console.WriteLine($"{s.Name}: {count} enrollments");
    }
  }

  // ═══════════════════════════════════════════════════════
  // PART B — Fix 1: Single query with projection 
  // SQL log: 1 query only — COUNT(*) subquery 
  // ═══════════════════════════════════════════════════════
  public async Task ShowProjectionFixAsync(CancellationToken ct = default)
  {
    Console.WriteLine("=== PART B Fix 1: Projection (GOOD) ===");

    // Single query — from EF Core s.Enrollments.Count translate to SQL subquery 
    // SELECT s.Name, (SELECT COUNT(*) FROM Enrollments WHERE StudentId = s.Id)
    var report = await _db.Students
        .AsNoTracking()
        .Select(s => new
        {
          s.Name,
          EnrollmentCount = s.Enrollments.Count  // ← SQL subquery 
        })
        .ToListAsync(ct);

    foreach (var r in report)
      Console.WriteLine($"{r.Name}: {r.EnrollmentCount} enrollments");


  }

  // ═══════════════════════════════════════════════════════
  // PART B — Fix 2: Include (full enrollment objects 
  // SQL log: 1 query — LEFT JOIN Enrollments
  // ═══════════════════════════════════════════════════════
  public async Task ShowIncludeFixAsync(CancellationToken ct = default)
  {
    Console.WriteLine("=== PART B Fix 2: Include (GOOD) ===");

    // Single query — LEFT JOIN "Enrollments" uses
    var students = await _db.Students
        .AsNoTracking()
        .Include(s => s.Enrollments)   // ← Enrollment objects call alls
        .ToListAsync(ct);

    foreach (var s in students)
      Console.WriteLine($"{s.Name}: {s.Enrollments.Count} enrollments");


  }
}