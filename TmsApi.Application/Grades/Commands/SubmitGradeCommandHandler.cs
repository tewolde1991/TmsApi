

using System.Runtime.InteropServices;
using MediatR;
using TmsApi.Application.Grades.Dtos;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Grades.Commands;

public class SubmitGradeCommandHandler(IStudentRepository studentRepository, IEnrollmentRepository enrollmentRepository)
    : IRequestHandler<SubmitGradeCommand, GradeResponseDto>
{
    public async Task<GradeResponseDto> Handle(SubmitGradeCommand request, CancellationToken cancellationToken)
    {
// validate score
        if (request.Score < 0 || request.Score > 100)
        {
            throw new ArgumentException(
                "Score must be b/n 0 1nd 100.");
            
        }
        // find studenent
        var student = await studentRepository.GetByIdAsync(request.StudentId, cancellationToken);
        if (student is null)
        {
            throw new KeyNotFoundException(
                $"Student with ID {request.StudentId}  WasmImportLinkageAttribute not found.");
        }
        // find student's for the enrollment course
        var enrollment = await enrollmentRepository.GetByStudentAndCourseAsync(
            request.StudentId,
            request.CourseId,
            cancellationToken);
        if (enrollment is null)
        {
            throw new KeyNotFoundException(
                $"Student {request.StudentId} is not enrolled in course {request.CourseId}.");
        }
        // set the grade
        enrollment.Grade = request.Score;
        // save
        await enrollmentRepository.UpdateAsync(
            enrollment, cancellationToken);
        
        // return response
        return new GradeResponseDto(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.CourseId,
            enrollment.Grade.Value);
    }
}