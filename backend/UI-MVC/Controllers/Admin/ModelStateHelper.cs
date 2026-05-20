using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Conversey.UI_MVC.Controllers.Admin;

public static class ModelStateHelper
{
    public static void ApplyValidationException(ModelStateDictionary modelState, ValidationException ex)
    {
        if (ex.Data.Contains("ValidationResults") && ex.Data["ValidationResults"] is IEnumerable<ValidationResult> results)
        {
            foreach (var validationResult in results)
            {
                if (validationResult.MemberNames.Any())
                {
                    foreach (var memberName in validationResult.MemberNames)
                    {
                        modelState.AddModelError(memberName, validationResult.ErrorMessage ?? "Invalid value");
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
}