using System.Threading.Channels;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TmsApi.Application.Transcripts;
using TmsApi.Infrastructure.Transcripts;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/transcripts")]
[ApiVersion("2.0")]
public class TranscriptsController(
    Channel<TranscriptRequest> channel,
    ITranscriptStatusStore statusStore) : ControllerBase
{
    [HttpPost]
    [MapToApiVersion("2.0")]
    [EnableRateLimiting("transcripts")]
    public async Task<IActionResult> RequestTranscript(
        TranscriptRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        // ── Idempotency check ─────────────────────────────────────────────
        // Same key = same reportId returned, no second worker job enqueued.
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await statusStore.GetReportIdForIdempotencyKeyAsync(idempotencyKey, ct);
            if (existing is not null)
            {
                var existingStatus = await statusStore.GetAsync(existing, ct);
                return Accepted(
                    Url.Action(nameof(GetStatus), new { id = existing }),
                    existingStatus);
            }
        }

        // ── Create new report ─────────────────────────────────────────────
        var reportId = Guid.NewGuid().ToString("N")[..12];
        var status = await statusStore.CreateAsync(reportId, request.StudentId, ct);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
            await statusStore.LinkIdempotencyKeyAsync(idempotencyKey, reportId, ct);

        // Enqueue — channel capacity 100, FullMode = Wait
        await channel.Writer.WriteAsync(request.WithReportId(reportId), ct);

        // Hint: poll again in ~5 s (simulated generation time)
        Response.Headers.RetryAfter = "5";

        return Accepted(
            Url.Action(nameof(GetStatus), new { id = reportId }),
            status);
    }

    [HttpGet("{id}/status")]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetStatus(string id, CancellationToken ct)
    {
        var status = await statusStore.GetAsync(id, ct);

        return status is null
            ? NotFound(new ProblemDetails
            {
                Title = "Transcript not found",
                Detail = $"No transcript request with id '{id}'.",
                Status = StatusCodes.Status404NotFound
            })
            : Ok(status);
    }
}