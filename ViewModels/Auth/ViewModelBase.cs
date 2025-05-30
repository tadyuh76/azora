using CommunityToolkit.Mvvm.ComponentModel;
using AvaloniaAzora.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Linq;
using ValidationResult = AvaloniaAzora.Services.ValidationResult;

namespace AvaloniaAzora.ViewModels;

public class ViewModelBase : ObservableObject
{
    protected readonly IValidationService? _validationService;
    private readonly Dictionary<string, List<string>> _propertyErrors = new();

    public ViewModelBase()
    {
        try
        {
            _validationService = ServiceProvider.GetService<IValidationService>();
        }
        catch
        {
            // ValidationService not available during design time or before DI initialization
            _validationService = null;
        }
    }

    /// <summary>
    /// Gets validation errors for a specific property
    /// </summary>
    public IEnumerable<string> GetErrors(string propertyName)
    {
        return _propertyErrors.ContainsKey(propertyName) ? _propertyErrors[propertyName] : new List<string>();
    }

    /// <summary>
    /// Checks if a property has validation errors
    /// </summary>
    public bool HasErrors(string propertyName)
    {
        return _propertyErrors.ContainsKey(propertyName) && _propertyErrors[propertyName].Count > 0;
    }

    /// <summary>
    /// Gets the first error for a property
    /// </summary>
    public string GetFirstError(string propertyName)
    {
        var errors = GetErrors(propertyName);
        return errors.FirstOrDefault() ?? string.Empty;
    }    /// <summary>
    /// Validates a property and updates the error collection
    /// </summary>
    protected virtual ValidationResult ValidateProperty<T>(T value, [CallerMemberName] string propertyName = "")
    {
        if (_validationService == null) return ValidationResult.Success();

        var result = _validationService.ValidateProperty(this, propertyName, value);
        UpdatePropertyErrors(propertyName, result);
        return result;
    }

    /// <summary>
    /// Validates the entire object using data annotations
    /// </summary>
    protected ValidationResult ValidateObject()
    {
        if (_validationService == null) return ValidationResult.Success();

        var result = _validationService.ValidateObject(this);
        
        // Clear all property errors first
        _propertyErrors.Clear();
        
        // Add errors from validation result
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                // Try to extract property name from error message, otherwise use general error
                AddError("General", error);
            }
        }        
        OnPropertyChanged(nameof(HasAnyErrors));
        return result;
    }

    /// <summary>
    /// Clears validation errors for a specific property
    /// </summary>
    protected void ClearPropertyErrors(string propertyName)
    {        if (_propertyErrors.ContainsKey(propertyName))
        {
            _propertyErrors.Remove(propertyName);
            OnPropertyChanged(nameof(HasAnyErrors));
        }
    }    /// <summary>
    /// Clears all validation errors
    /// </summary>
    protected void ClearAllErrors()
    {
        _propertyErrors.Clear();
        OnPropertyChanged(nameof(HasAnyErrors));
    }

    /// <summary>
    /// Adds an error for a specific property
    /// </summary>
    protected void AddError(string propertyName, string error)
    {
        if (!_propertyErrors.ContainsKey(propertyName))
        {
            _propertyErrors[propertyName] = new List<string>();
        }
          if (!_propertyErrors[propertyName].Contains(error))
        {
            _propertyErrors[propertyName].Add(error);
            OnPropertyChanged(nameof(HasAnyErrors));
        }
    }

    /// <summary>
    /// Adds an error for a specific property (alias for AddError)
    /// </summary>
    protected void AddPropertyError(string propertyName, string error)
    {
        AddError(propertyName, error);
    }

    /// <summary>
    /// Updates property errors based on validation result
    /// </summary>
    private void UpdatePropertyErrors(string propertyName, ValidationResult result)
    {
        // Clear existing errors for this property
        ClearPropertyErrors(propertyName);
        
        // Add new errors if validation failed
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                AddError(propertyName, error);
            }
        }
    }    /// <summary>
    /// Indicates whether the ViewModel has any validation errors
    /// </summary>
    public bool HasAnyErrors => _propertyErrors.Any(kvp => kvp.Value.Count > 0);
}
