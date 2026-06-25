using Microsoft.AspNetCore.Mvc;
using TmsApi.Services;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentReportController : ControllerBase
{
  private readonly EnrollmentReportService _svc;

  public EnrollmentReportController(EnrollmentReportService svc) => _svc = svc;

  // GET /api/enrollmentreport/nplusone
  [HttpGet("nplusone")]
  public async Task<IActionResult> NPlusOne(CancellationToken ct)
  {
    await _svc.ShowNPlusOneAsync(ct);
    return Ok("N+1 done — check SQL log (with in terminal )");
  }

  // GET /api/enrollmentreport/projection
  [HttpGet("projection")]
  public async Task<IActionResult> Projection(CancellationToken ct)
  {
    await _svc.ShowProjectionFixAsync(ct);
    return Ok("Projection done — only 1 query  check SQL log ");
  }

  // GET /api/enrollmentreport/include
  [HttpGet("include")]
  public async Task<IActionResult> Include(CancellationToken ct)
  {
    await _svc.ShowIncludeFixAsync(ct);
    return Ok("Include done — LEFT JOIN SQL log check");
  }
}