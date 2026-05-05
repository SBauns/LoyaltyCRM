using System;
using System.ComponentModel.DataAnnotations;

public class AtLeastOneRequiredAttribute : ValidationAttribute
{
    private readonly string[] _propertyNames;

    public AtLeastOneRequiredAttribute(params string[] propertyNames)
    {
        _propertyNames = propertyNames;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        foreach (var propertyName in _propertyNames)
        {
            var property = validationContext.ObjectType.GetProperty(propertyName);
            if (property == null)
            {
                return new ValidationResult($"Unknown property: {propertyName}");
            }

            var propertyValue = property.GetValue(validationContext.ObjectInstance);
            if (propertyValue != null && !string.IsNullOrEmpty(propertyValue.ToString()))
            {
                return ValidationResult.Success;
            }
        }

        return new ValidationResult(ErrorMessage ?? "At least one of the fields is required.");
    }
}