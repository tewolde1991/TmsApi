namespace TmsApi.Models;

public class Grade
{
    public string StudentId { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public decimal Value { get; set; }
}