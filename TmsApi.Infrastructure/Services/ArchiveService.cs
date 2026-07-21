using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Infrastructure.Services;

public class ArchiveService
{
    private readonly TmsDbContext _db;

    public ArchiveService(TmsDbContext db) => _db = db;

    // ── TODO: Bulk archive — ExecuteUpdateAsync ──────────────────
    // Single UPDATE statement — rows 
    public async Task<int> BulkArchiveEnrollmentsAsync(
        DateTime cutoff, CancellationToken ct = default)
    {
        // SQL: UPDATE "Enrollments" SET "IsArchived" = true
        //      WHERE "EnrolledAt" < cutoff AND "IsArchived" = false
        var count = await _db.Enrollments
            .IgnoreQueryFilters()                        // archived rows show
            .Where(e => e.EnrolledAt < cutoff
                     && !e.IsArchived)
            .ExecuteUpdateAsync(
                s => s.SetProperty(e => e.IsArchived, true), ct);

        return count;
    }

    // ── Soft-delete student ──────────────────────────────────────
    public async Task<string> SoftDeleteStudentAsync(
        int id, CancellationToken ct = default)
    {

        var count = await _db.Students
            .IgnoreQueryFilters()
            .Where(s => s.Id == id)
            .ExecuteUpdateAsync(
                s => s.SetProperty(e => e.IsDeleted, true), ct);

        return count > 0
            ? $"✅ Student {id} soft-deleted"
            : $"❌ Student {id} not found";
    }

    // ── Normal query — soft-deleted remove ───────────────────────
    public async Task<List<string>> GetActiveStudentsAsync(
        CancellationToken ct = default)
    {
        return await _db.Students
            .AsNoTracking()
            .Select(s => $"{s.Name} (GPA: {s.GPA})")
            .ToListAsync(ct);
    }

    // ── Admin query — IgnoreQueryFilters() ─────────────────────
    // Soft-deleted students show
    public async Task<List<string>> GetAllStudentsAdminAsync(
        CancellationToken ct = default)
    {
        return await _db.Students
            .IgnoreQueryFilters()                        // filter ignore
            .AsNoTracking()
            .Select(s => $"{s.Name} | Deleted: {s.IsDeleted}")
            .ToListAsync(ct);
    }

    // ── Admin restore — start soft-delete  ───────────────────────
    public async Task<string> RestoreStudentAsync(
        int id, CancellationToken ct = default)
    {
        var count = await _db.Students
            .IgnoreQueryFilters()
            .Where(s => s.Id == id && s.IsDeleted)
            .ExecuteUpdateAsync(
                s => s.SetProperty(e => e.IsDeleted, false), ct);

        return count > 0
            ? $"✅ Student {id} restored"
            : $"❌ Student {id} not found or not deleted";
    }
}