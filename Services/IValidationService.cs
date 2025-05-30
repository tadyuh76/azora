using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AvaloniaAzora.Services
{    public interface IValidationService
    {
        ValidationResult ValidateObject(object obj);
        ValidationResult ValidateProperty(object obj, string propertyName, object? value);
        bool IsValidEmail(string email);
        bool IsValidPassword(string password);
        bool IsValidPhoneNumber(string phoneNumber);
        bool IsValidUrl(string url);
        ValidationResult ValidateString(string? value, int minLength = 0, int maxLength = int.MaxValue, bool required = false, string propertyName = "Field");
        ValidationResult ValidateNumber(double value, double min = double.MinValue, double max = double.MaxValue, bool required = false, string propertyName = "Field");
        ValidationResult ValidateDate(DateTime? value, DateTime? minDate = null, DateTime? maxDate = null, bool required = false, string propertyName = "Field");
        ValidationResult ValidateDate(DateTimeOffset? value, DateTimeOffset? minDate = null, DateTimeOffset? maxDate = null, bool required = false, string propertyName = "Field");
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public string FirstError => Errors.Count > 0 ? Errors[0] : string.Empty;

        public static ValidationResult Success() => new() { IsValid = true };
        public static ValidationResult Failure(string error) => new() { IsValid = false, Errors = { error } };
        public static ValidationResult Failure(List<string> errors) => new() { IsValid = false, Errors = errors };
    }
}
