
using FluentValidation;

namespace Objectware.MinimalAPI.NewAPI.Validation;
public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext ctx,
        EndpointFilterDelegate next)
    {
        var model = ctx.Arguments.OfType<T>().FirstOrDefault();
        if (model is null)
            return TypedResults.BadRequest("Invalid request body");

        var result = await validator.ValidateAsync(model);
        if (!result.IsValid)
            return TypedResults.ValidationProblem(result.ToDictionary());

        return await next(ctx);
    }
}
