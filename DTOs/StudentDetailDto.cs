namespace TmsApi.Dtos;

public record StudentDetailDto
{
  public required int Id { get; init; }
  public required string Name { get; init; }
  public required string Email { get; init; }
  public required int EnrollmentCount { get; init; }
  public required IReadOnlyList<LinkDto> Links { get; init; }
}