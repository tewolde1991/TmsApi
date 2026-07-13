using Microsoft.AspNetCore.Mvc;
using Tms.Api.Dtos;
using TmsApi.Services;

namespace Tms.Api.Controllers;

[ApiController]
[Route("api/students/{studentId:int}/certificates")]
[Tags("Certificates")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CertificatesController(
    IStudentService studentService,
    ICertificateService certificateService) : ControllerBase
{
    [HttpGet(Name = "ListStudentCertificates")]
    [ProducesResponseType(typeof(IReadOnlyList<CertificateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("List certificates for a student")]
    public async Task<IActionResult> GetCertificates(int studentId, CancellationToken ct)
    {
        var student = await studentService.GetByIdAsync(studentId, ct);
        if (student is null) return NotFound();

        var certificates = await certificateService.GetByStudentAsync(studentId, ct);
        return Ok(certificates);
    }

    [HttpGet("{id:int}", Name = nameof(GetCertificate))]
    [ProducesResponseType(typeof(CertificateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get one certificate for a student")]
    public async Task<IActionResult> GetCertificate(int studentId, int id, CancellationToken ct)
    {
        var certificate = await certificateService.GetByIdAsync(studentId, id, ct);
        return certificate is not null ? Ok(certificate) : NotFound();
    }


    [HttpPost]
    [ProducesResponseType(typeof(CertificateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [EndpointSummary("Issue a certificate to a student")]
    [EndpointDescription("Returns 404 if the student or course does not exist, 409 if a certificate was already issued for this course.")]
    public async Task<IActionResult> IssueCertificate(int studentId, IssueCertificateRequest request, CancellationToken ct)
    {
        try
        {
            var result = await certificateService.IssueAsync(studentId, request, ct);
            return CreatedAtAction(nameof(GetCertificate), new { studentId, id = result.Id }, result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new ProblemDetails { Title = "Resource not found", Detail = ex.Message, Status = 404 });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already been issued"))
        {
            return Conflict(new ProblemDetails { Title = "Certificate already issued", Detail = ex.Message, Status = 409 });
        }
    }
}