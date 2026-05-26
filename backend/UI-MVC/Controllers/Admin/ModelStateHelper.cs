using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Conversey.UI_MVC.Controllers.Admin;

public static class ModelStateHelper
{
    public static void ApplyValidationException(
        ModelStateDictionary modelState,
        ValidationException ex,
        string? memberPrefix = null)
    {
        if (ex.Data.Contains("ValidationResults") && ex.Data["ValidationResults"] is IEnumerable<ValidationResult> results)
        {
            foreach (var validationResult in results)
            {
                if (validationResult.MemberNames.Any())
                {
                    foreach (var memberName in validationResult.MemberNames)
                    {
                        var key = string.IsNullOrEmpty(memberPrefix) ? memberName : $"{memberPrefix}.{memberName}";
                        modelState.AddModelError(key, validationResult.ErrorMessage ?? "Invalid value");
                    }
                }
                else
                {
                    modelState.AddModelError(string.Empty, validationResult.ErrorMessage ?? "Validation failed");
                }
            }
        }
        else
        {
            modelState.AddModelError(string.Empty, ex.Message);
        }
    }

    public static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return parsed;
        return null;
    }
}