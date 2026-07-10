namespace TmsApi.Dtos;

// Detail DTO — single course + HATEOAS links
// List response still uses CourseResponseDto (no per-item links)
public record CourseDetailDto
{
  public required int Id { get; init; }
  public required string Code { get; init; }
  public required string Title { get; init; }
  public required int MaxCapacity { get; init; }
  public required int EnrollmentCount { get; init; }

  // HATEOAS links — what can client do next?
  public required IReadOnlyList<LinkDto> Links { get; init; }
}