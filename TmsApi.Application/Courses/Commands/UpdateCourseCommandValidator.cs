using FluentValidation;

namespace TmsApi.Application.Courses.Commands;

public class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.MaxCapacity).GreaterThan(0);
    }
}