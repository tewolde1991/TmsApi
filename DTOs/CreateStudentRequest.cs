using System.ComponentModel.DataAnnotations;

namespace TmsApi.Dtos;

public record CreateStudentRequest
{
  [Required]
  [MaxLength(20)]
  public required string RegistrationNumber { get; init; }

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
  public decimal GPA { get; init; } = 0;
}