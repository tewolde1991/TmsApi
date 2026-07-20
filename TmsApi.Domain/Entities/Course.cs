
// M6 Start
namespace TmsApi.Domain.Entities;

public class Course
{
  public int Id { get; set; }
  public required string Code { get; set; }
  public required string Title { get; set; }
  public int MaxCapacity { get; set; }  // ← Capacity ተቀይሯል

  public ICollection<Enrollment> Enrollments { get; set; } = [];
}