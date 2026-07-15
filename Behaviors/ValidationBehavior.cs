


using FluentValidation;
using MediatR;

public class ValidationBehavior<TRequest, TReponse>(IEnumerable<IValidator<TRequest>> validators): IPipelineBehavior<TRequest, TReponse> where TRequest: notnull
{
    public async Task<TReponse> Handle(
        TRequest request, RequestHandlerDelegate<TReponse> next, CancellationToken ct
    )
    {
        if (!validators.Any())
        return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = validators
                        .Select(v=>v.Validate(context))
                        .SelectMany(result => result.Errors)
                        .Where(f=>f is not null)
                        .ToList();

            if (failures.Count > 0)
                    throw new ValidationException(failures);

                return await next();
    }
}