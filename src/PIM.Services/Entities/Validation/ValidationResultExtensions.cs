using FluentValidation.Results;

namespace PIM.Services.Entities.Validation;

public static class ValidationResultExtensions
{
    public static IDictionary<string, string> ToInputErrors(this ValidationResult result)
        => result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => string.Join(" ", g.Select(e => e.ErrorMessage)));
}
