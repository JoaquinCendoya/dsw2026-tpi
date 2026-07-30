using AppValidationException = Dsw2026Tpi.CrossCutting.Exceptions.ValidationException;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dsw2026Tpi.Api.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var argumentType = argument.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);

            if (context.HttpContext.RequestServices.GetService(validatorType) is IValidator validator)
            {
                var validationContext = new ValidationContext<object>(argument);
                var result = await validator.ValidateAsync(validationContext);

                if (!result.IsValid)
                {
                    var exception = new AppValidationException();
                    exception.WithDetail(result.Errors.Select(e => (e.PropertyName, e.ErrorMessage)));
                    throw exception;
                }
            }
        }

        await next();
    }
}