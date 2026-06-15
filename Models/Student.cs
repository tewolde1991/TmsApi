namespace TmsApi.Models;

public class Student
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public decimal Gpa { get; set; }
}