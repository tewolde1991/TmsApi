namespace TmsApi.Application.DTOs;

public record UpdateStudentRequest(string Name, decimal GPA,bool IsActive, uint Version );