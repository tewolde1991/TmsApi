namespace TmsApi.Models;

public class Course
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Capacity { get; set; }
}