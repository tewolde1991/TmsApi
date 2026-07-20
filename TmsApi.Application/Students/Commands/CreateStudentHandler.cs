using MediatR;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Students.Commands;

public class CreateStudentHandler(IStudentRepository studRepo)
    : IRequestHandler<CreateStudentCommand, StudentCreatedDto>
{
    public async Task<StudentCreatedDto> Handle(CreateStudentCommand command, CancellationToken ct)
    {
        var student = new Student
        {
            Name = command.Name,
            RegistrationNumber = command.RegistrationNumber,
            IsActive = command.IsActive
        };
        await studRepo.AddAsync(student, ct);

        return new StudentCreatedDto(
            student.Id,
            student.Name,
            student.RegistrationNumber,
            student.IsActive);
    }
}