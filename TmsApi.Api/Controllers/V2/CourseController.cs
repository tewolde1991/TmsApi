using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Dtos;
using TmsApi.Infrastructure.Persistence;
using TmsApi.Api.RateLimiting;

namespace TmsApi.Api.Controllers.v2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class CoursesController : ControllerBase
{
  private readonly TmsDbContext _context;

  public CoursesController(TmsDbContext context)
  {
    _context = context;
  }

  [HttpGet]
  public async Task<IActionResult> GetCourses(
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 20,
      CancellationToken ct = default)
  {
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 1, 50);

    var baseQuery = _context.Courses.AsNoTracking();
    var totalCount = await baseQuery.CountAsync(ct);

    var rows = await baseQuery
        .OrderBy(c => c.Title)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(c => new
        {
          c.Id,
          c.Title,
          c.Code,
          c.MaxCapacity,
          EnrollmentCount = c.Enrollments.Count
        })
        .ToListAsync(ct);

    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    var hasNext = page < totalPages;
    var hasPrevious = page > 1;

    return Ok(new
    {
      data = rows,
      meta = new { totalCount, page, pageSize, totalPages, hasNext, hasPrevious },
      links = new
      {
        self = $"/api/v2/courses?page={page}&pageSize={pageSize}",
        next = hasNext ? $"/api/v2/courses?page={page + 1}&pageSize={pageSize}" : (string?)null,
        prev = hasPrevious ? $"/api/v2/courses?page={page - 1}&pageSize={pageSize}" : (string?)null,
        enroll = "/api/v2/enrollments"
      }
    });
  }

  [HttpGet("search")]
  [EnableRateLimiting("search")]
  public async Task<IActionResult> SearchCourses(
      [FromQuery] string? term,
      CancellationToken ct)
  {
    var query = _context.Courses.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(term))
    {
      query = query.Where(c =>
          c.Title.Contains(term, StringComparison.OrdinalIgnoreCase)
          || c.Code.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    var results = await query
        .Select(c => new
        {
          c.Id,
          c.Title,
          c.Code,
          c.MaxCapacity,
          EnrollmentCount = c.Enrollments.Count
        })
        .ToListAsync(ct);

    return Ok(new { data = results, totalCount = results.Count });
  }
}