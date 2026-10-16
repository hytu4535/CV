using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ShopUI.Pages.Admin.Products
{
    public class GreaterThanPropertyAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly string _otherProperty;

        public GreaterThanPropertyAttribute(string otherProperty)
        {
            _otherProperty = otherProperty;
            ErrorMessage = "The {0} must be greater than {1}.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var otherProperty = validationContext.ObjectType.GetProperty(_otherProperty);
            if (otherProperty == null)
            {
                return new ValidationResult(string.Format("Unknown property: {0}", _otherProperty));
            }

            var otherValue = otherProperty.GetValue(validationContext.ObjectInstance);

            if (value is decimal d && otherValue is decimal otherD)
            {
                if (d <= otherD)
                {
                    return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
                }
            }

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-greaterthan", ErrorMessage ?? $"The field {context.ModelMetadata.GetDisplayName()} must be greater than {_otherProperty}.");
            MergeAttribute(context.Attributes, "data-val-greaterthan-other", _otherProperty);
        }

        private bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
            {
                return false;
            }
            attributes.Add(key, value);
            return true;
        }
    }
}