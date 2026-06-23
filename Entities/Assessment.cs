namespace TmsApi.Entities;

public class Assessment
{
  public int Id { get; set; }
  public required string Title { get; set; }
  public decimal MaxScore { get; set; }
  public decimal weight { get; set; }

  public int CourseId { get; set; }
  public int StudentId { get; set; }
  // navigation
  public required Course Course
  { get; set; } = null!;
  public Student Student { get; set; } = null!;
}