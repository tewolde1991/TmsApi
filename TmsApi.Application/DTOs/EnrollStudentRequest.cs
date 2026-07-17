

using System.ComponentModel.DataAnnotations;

namespace TmsApi.Application.DTOs;

public record EnrollStudentRequest(int StudentId, string CourseCode)
{
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a positive integer.")]
    public required int StudentId {get; init;}
}