using Microsoft.AspNetCore.Mvc;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentUpdateController : ControllerBase
{
  private readonly StudentUpdateService _svc;

  public StudentUpdateController(StudentUpdateService svc) => _svc = svc;

  // PUT /api/studentupdate/1/name?firstName=Alice&lastName=Updated
  [HttpPut("{id}/name")]
  public async Task<IActionResult> UpdateName(
      int id, [FromQuery] string firstName, [FromQuery] string lastName, CancellationToken ct)
  {
    var result = await _svc.UpdateNameAsync(id, firstName, lastName, ct);
    return Ok(result);
  }

  // PUT /api/studentupdate/1/gpa?value=3.9
  [HttpPut("{id}/gpa")]
  public async Task<IActionResult> UpdateGpa(
      int id, [FromQuery] decimal value, CancellationToken ct)
  {
    var result = await _svc.UpdateGpaAsync(id, value, ct);
    return Ok(result);
  }

  // GET /api/studentupdate/1/lastupdated
  [HttpGet("{id}/lastupdated")]
  public async Task<IActionResult> GetLastUpdated(int id, CancellationToken ct)
  {
    var result = await _svc.GetLastUpdatedAsync(id, ct);
    return Ok(result);
  }
}