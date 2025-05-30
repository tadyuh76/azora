using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace AvaloniaAzora.ViewModels
{    public partial class CreateClassroomViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _className = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        public CreateClassroomViewModel()
        {
        }

        partial void OnClassNameChanged(string value)
        {
            ValidateProperty(value);
        }

        partial void OnDescriptionChanged(string value)
        {
            ValidateProperty(value);
        }        protected new void ValidateProperty<T>(T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null) return;

            ClearPropertyErrors(propertyName);

            switch (propertyName)
            {
                case nameof(ClassName):
                    var classNameValidation = _validationService?.ValidateString(value?.ToString(), 3, 100, true);
                    if (classNameValidation != null && !classNameValidation.IsValid)
                    {
                        foreach (var error in classNameValidation.Errors)
                        {
                            AddPropertyError(propertyName, error);
                        }
                    }
                    break;

                case nameof(Description):
                    if (!string.IsNullOrEmpty(value?.ToString()))
                    {
                        var descriptionValidation = _validationService?.ValidateString(value.ToString(), 0, 500, false);
                        if (descriptionValidation != null && !descriptionValidation.IsValid)
                        {
                            foreach (var error in descriptionValidation.Errors)
                            {
                                AddPropertyError(propertyName, error);
                            }
                        }
                    }
                    break;
            }
        }

        [RelayCommand]
        private async Task ValidateForm()
        {
            // Clear all errors first
            ClearAllErrors();

            // Validate all properties
            ValidateProperty(ClassName, nameof(ClassName));
            ValidateProperty(Description, nameof(Description));

            // Additional business logic validation
            if (string.IsNullOrWhiteSpace(ClassName))
            {
                AddPropertyError(nameof(ClassName), "Class name is required");
            }
            else if (ClassName.Trim().Length < 3)
            {
                AddPropertyError(nameof(ClassName), "Class name must be at least 3 characters long");
            }
            else if (ClassName.Trim().Length > 100)
            {
                AddPropertyError(nameof(ClassName), "Class name cannot exceed 100 characters");
            }

            await Task.CompletedTask;
        }

        public bool IsFormValid()        {
            ValidateForm().Wait();
            return !HasAnyErrors;
        }
    }
}