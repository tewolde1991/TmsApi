using Microsoft.AspNetCore.Mvc;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArchiveController : ControllerBase
{
  private readonly ArchiveService _svc;
  public ArchiveController(ArchiveService svc) => _svc = svc;

  // POST /api/archive/enrollments?cutoffDays=30
  [HttpPost("enrollments")]
  public async Task<IActionResult> BulkArchive(
      [FromQuery] int cutoffDays = 30, CancellationToken ct = default)
  {
    var cutoff = DateTime.UtcNow.AddDays(-cutoffDays);
    var count = await _svc.BulkArchiveEnrollmentsAsync(cutoff, ct);
    return Ok($"✅ {count} enrollments archived (older than {cutoffDays} days)");
  }

  // DELETE /api/archive/students/1
  [HttpDelete("students/{id}")]
  public async Task<IActionResult> SoftDelete(int id, CancellationToken ct)
  {
    var result = await _svc.SoftDeleteStudentAsync(id, ct);
    return Ok(result);
  }

  // GET /api/archive/students/active
  [HttpGet("students/active")]
  public async Task<IActionResult> GetActive(CancellationToken ct)
  {
    var list = await _svc.GetActiveStudentsAsync(ct);
    return Ok(list);
  }

  // GET /api/archive/students/admin
  [HttpGet("students/admin")]
  public async Task<IActionResult> GetAdmin(CancellationToken ct)
  {
    var list = await _svc.GetAllStudentsAdminAsync(ct);
    return Ok(list);
  }

  // POST /api/archive/students/1/restore
  [HttpPost("students/{id}/restore")]
  public async Task<IActionResult> Restore(int id, CancellationToken ct)
  {
    var result = await _svc.RestoreStudentAsync(id, ct);
    return Ok(result);
  }
}