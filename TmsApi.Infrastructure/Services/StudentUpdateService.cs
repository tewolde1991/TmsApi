using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Services;

public class StudentUpdateService
{
  private readonly TmsDbContext _db;

  public StudentUpdateService(TmsDbContext db) => _db = db;

  public async Task<string> UpdateNameAsync(
      int id, string newFirstName, string newLastName, CancellationToken ct = default)
  {
    var student = await _db.Students.FindAsync([id], ct);
    if (student is null) return $"Student {id} not found";

    student.FirstName = newFirstName;
    student.LastName = newLastName;

    _db.Entry(student)
       .Property("LastUpdated")
       .CurrentValue = DateTime.UtcNow;

    try
    {
      await _db.SaveChangesAsync(ct);
      return $"✅ Updated: {student.Name} — LastUpdated set to {DateTime.UtcNow:u}";
    }
    catch (DbUpdateConcurrencyException ex)
    {
      // Exercise 8b — concurrency conflict ተከሰተ
      var entry = ex.Entries.Single();
      var db = await entry.GetDatabaseValuesAsync(ct);
      var dbFirstName = db?["FirstName"];
      var dbLastName = db?["LastName"];
      return $"❌ Concurrency conflict! DB has: FirstName='{dbFirstName}', LastName='{dbLastName}' — reload and retry.";
    }
  }

  // ── Update GPA — concurrency conflict for testing ──
  public async Task<string> UpdateGpaAsync(
      int id, decimal newGpa, CancellationToken ct = default)
  {
    var student = await _db.Students.FindAsync([id], ct);
    if (student is null) return $"Student {id} not found";

    student.GPA = newGpa;

    _db.Entry(student)
       .Property("LastUpdated")
       .CurrentValue = DateTime.UtcNow;

    try
    {
      await _db.SaveChangesAsync(ct);
      return $"✅ Updated GPA: {student.GPA}";
    }
    catch (DbUpdateConcurrencyException ex)
    {
      var entry = ex.Entries.Single();
      var db = await entry.GetDatabaseValuesAsync(ct);
      var dbGpa = db?["GPA"];
      return $"❌ Concurrency conflict! DB has: GPA='{dbGpa}' — reload and retry.";
    }
  }

  // ── LastUpdated shadow property ──
  public async Task<string> GetLastUpdatedAsync(int id, CancellationToken ct = default)
  {
    var student = await _db.Students.FindAsync([id], ct);
    if (student is null) return $"Student {id} not found";

    var lastUpdated = _db.Entry(student)
                         .Property("LastUpdated")
                         .CurrentValue;

    return $"Student: {student.Name} | LastUpdated: {lastUpdated}";
  }
}