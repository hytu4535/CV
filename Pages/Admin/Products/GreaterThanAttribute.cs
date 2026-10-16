using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ShopUI.Pages.Admin.Products
{
    public class GreaterThanPropertyAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly string _comparisonProperty;

        public GreaterThanPropertyAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
            ErrorMessage = "Giá cũ phải lớn hơn giá bán hiện tại.";
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var currentValue = (decimal)value;

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);
            if (property == null)
            {
                throw new ArgumentException("Property with this name not found");
            }

            var comparisonValue = (decimal)property.GetValue(validationContext.ObjectInstance);

            if (currentValue <= comparisonValue)
            {
                return new ValidationResult(ErrorMessage);
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
            MergeAttribute(context.Attributes, "data-val-greaterthanproperty", ErrorMessage);
            MergeAttribute(context.Attributes, "data-val-greaterthanproperty-other", "*#" + _comparisonProperty);
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