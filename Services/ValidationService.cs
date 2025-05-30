using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace AvaloniaAzora.Services
{
    public class ValidationService : IValidationService
    {
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        private static readonly Regex PhoneRegex = new(@"^\+?[1-9]\d{1,14}$");
        private static readonly Regex UrlRegex = new(@"^https?://.+", RegexOptions.IgnoreCase);

        public ValidationResult ValidateObject(object obj)
        {
            var validationContext = new ValidationContext(obj);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            
            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResults, true);
            
            if (isValid)
                return ValidationResult.Success();
            
            var errors = validationResults.Select(vr => vr.ErrorMessage ?? "Validation error").ToList();
            return ValidationResult.Failure(errors);
        }

        public ValidationResult ValidateProperty(object obj, string propertyName, object? value)
        {
            var validationContext = new ValidationContext(obj) { MemberName = propertyName };
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            
            bool isValid = Validator.TryValidateProperty(value, validationContext, validationResults);
            
            if (isValid)
                return ValidationResult.Success();
            
            var errors = validationResults.Select(vr => vr.ErrorMessage ?? "Validation error").ToList();
            return ValidationResult.Failure(errors);
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            
            return EmailRegex.IsMatch(email) && email.Length <= 254; // RFC 5322 limit
        }

        public bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;
            
            // Password requirements: at least 6 characters
            // You can enhance this with more complex requirements
            return password.Length >= 6 && password.Length <= 128;
        }

        public bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;
            
            // Remove common formatting characters
            var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            return PhoneRegex.IsMatch(cleaned);
        }

        public bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;
            
            return UrlRegex.IsMatch(url) && Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        public ValidationResult ValidateString(string? value, int minLength = 0, int maxLength = int.MaxValue, bool required = false, string propertyName = "Field")
        {
            if (required && string.IsNullOrWhiteSpace(value))
                return ValidationResult.Failure($"{propertyName} is required.");
            
            if (!string.IsNullOrWhiteSpace(value))
            {
                var trimmedLength = value.Trim().Length;
                
                if (trimmedLength < minLength)
                    return ValidationResult.Failure($"{propertyName} must be at least {minLength} characters long.");
                
                if (trimmedLength > maxLength)
                    return ValidationResult.Failure($"{propertyName} cannot exceed {maxLength} characters.");
            }
            
            return ValidationResult.Success();
        }

        public ValidationResult ValidateNumber(double value, double min = double.MinValue, double max = double.MaxValue, bool required = false, string propertyName = "Field")
        {
            if (required && (double.IsNaN(value) || double.IsInfinity(value)))
                return ValidationResult.Failure($"{propertyName} is required.");
            
            if (!double.IsNaN(value) && !double.IsInfinity(value))
            {
                if (value < min)
                    return ValidationResult.Failure($"{propertyName} must be at least {min}.");
                
                if (value > max)
                    return ValidationResult.Failure($"{propertyName} cannot exceed {max}.");
            }
            
            return ValidationResult.Success();
        }        public ValidationResult ValidateDate(DateTime? value, DateTime? minDate = null, DateTime? maxDate = null, bool required = false, string propertyName = "Field")
        {
            if (required && !value.HasValue)
                return ValidationResult.Failure($"{propertyName} is required.");
            
            if (value.HasValue)
            {
                if (minDate.HasValue && value.Value < minDate.Value)
                    return ValidationResult.Failure($"{propertyName} cannot be earlier than {minDate.Value:yyyy-MM-dd}.");
                
                if (maxDate.HasValue && value.Value > maxDate.Value)
                    return ValidationResult.Failure($"{propertyName} cannot be later than {maxDate.Value:yyyy-MM-dd}.");
            }
            
            return ValidationResult.Success();
        }

        public ValidationResult ValidateDate(DateTimeOffset? value, DateTimeOffset? minDate = null, DateTimeOffset? maxDate = null, bool required = false, string propertyName = "Field")
        {
            if (required && !value.HasValue)
                return ValidationResult.Failure($"{propertyName} is required.");
            
            if (value.HasValue)
            {
                if (minDate.HasValue && value.Value < minDate.Value)
                    return ValidationResult.Failure($"{propertyName} cannot be earlier than {minDate.Value:yyyy-MM-dd}.");
                
                if (maxDate.HasValue && value.Value > maxDate.Value)
                    return ValidationResult.Failure($"{propertyName} cannot be later than {maxDate.Value:yyyy-MM-dd}.");
            }
            
            return ValidationResult.Success();
        }
    }
}
