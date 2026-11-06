using Dsw2026Tpi.Domain.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dsw2026Tpi.Api.Binders;

public class AppointmentStatusModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        var value = valueProviderResult.FirstValue;
        if (string.IsNullOrWhiteSpace(value))
        {
            return Task.CompletedTask;
        }

        // Remueve guiones bajos para soportar 'NO_SHOW' mapeando a 'NoShow'
        var normalized = value.Trim().Replace("_", "");
        if (Enum.TryParse<AppointmentStatus>(normalized, true, out var status))
        {
            bindingContext.Result = ModelBindingResult.Success(status);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                $"El valor '{value}' no es un estado válido de turno."
            );
        }

        return Task.CompletedTask;
    }
}

public class AppointmentStatusModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(AppointmentStatus) ||
            context.Metadata.ModelType == typeof(AppointmentStatus?))
        {
            return new AppointmentStatusModelBinder();
        }

        return null;
    }
}
