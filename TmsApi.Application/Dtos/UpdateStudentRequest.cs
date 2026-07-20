using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.Dtos;

public record UpdateStudentRequest
{
    [Required]
    [MaxLength(100)]
    public required string FirstName { get; init; }

    [Required]
    [MaxLength(100)]
    public required string LastName { get; init; }

    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Range(0.0, 4.0)]
    public decimal GPA { get; init; }

    public bool IsActive { get; init; } = true;
}