using System.ComponentModel.DataAnnotations.Schema;

namespace TmsApi.Domain.Entities;

public class Student
{
  public int Id { get; set; }

  public required string RegistrationNumber { get; set; }
  public required string FirstName { get; set; }
  public required string LastName { get; set; }
  public required string Email { get; set; }

  [NotMapped]
  public string Name => $"{FirstName} {LastName}";

  public decimal GPA { get; set; }
  public bool IsDeleted { get; set; } = false;
  public bool IsActive { get; set; } = true;

  public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
  public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

  public uint RowVersion { get; set; }
}