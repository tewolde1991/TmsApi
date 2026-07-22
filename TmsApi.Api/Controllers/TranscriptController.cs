using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TmsApi.Api.Controllers;

[ApiController]
[Route("api/v2/transcripts")]
public class TranscriptsController : ControllerBase
{
  // [EnableRateLimiting] → named policy applies
  // GlobalLimiter also applies — both must pass
  [HttpPost]
  [EnableRateLimiting("transcripts")]
  public IActionResult RequestTranscript([FromBody] object? _)
  {
    // Stub — Exercise 5 iy 202 Accepted + background job iy iyቀይሩ
    return Ok(new { message = "Transcript queued" });
  }
}