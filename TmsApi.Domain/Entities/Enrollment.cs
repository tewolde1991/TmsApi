namespace TmsApi.Domain.Entities;

public class Enrollment
{
    public int Id {get;set;}
    public required int StudentId {get; set;}
    public required int CourseId {get; set;}
    public decimal? Grade {get; set;}

    public DateTime EnrolledAt {get; set;} = DateTime.UtcNow;
    public int Year { get; set; } = DateTime.UtcNow.Year;
    public bool IsArchived {get; set;} = false;

    public Student Student {get; set;} = null!;
    public Course Course {get; set;} = null!;
}