namespace Tms.Api.Dtos;

public record UpdateStudentRequest(string Name, decimal GPA,bool IsActive, uint Version );