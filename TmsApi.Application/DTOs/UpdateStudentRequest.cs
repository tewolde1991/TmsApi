namespace TmsApi.Application.DTOs;

public record UpdateStudentRequest(
    string FirstName,
    string LastName,
    decimal GPA,
    bool IsActive,
    uint Version);