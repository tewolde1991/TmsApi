using Microsoft.AspNetCore.Mvc;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
  private readonly DashboardService _svc;

  public DashboardController(DashboardService svc) => _svc = svc;

  // GET /api/dashboard/students?page=1
  [HttpGet("students")]
  public async Task<IActionResult> GetStudents(
      [FromQuery] int page = 1,
      CancellationToken ct = default)
  {
    var (rows, total) = await _svc.GetPagedStudentsAsync(page, ct);
    return Ok(new { rows, total, page });
  }

  // GET /api/dashboard/top-courses
  [HttpGet("top-courses")]
  public async Task<IActionResult> GetTopCourses(
      CancellationToken ct = default)
  {
    var data = await _svc.GetTop5CoursesByEnrollmentAsync(ct);
    return Ok(data);
  }
}