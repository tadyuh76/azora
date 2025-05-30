using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace AvaloniaAzora.ViewModels
{    public partial class CreateTestViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private int _timeLimit = 0;        public CreateTestViewModel()
        {
        }

        partial void OnTitleChanged(string value)
        {
            ValidateProperty(value);
        }

        partial void OnDescriptionChanged(string value)
        {
            ValidateProperty(value);
        }

        partial void OnTimeLimitChanged(int value)
        {
            ValidateProperty(value);
        }

        protected new void ValidateProperty<T>(T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null) return;

            ClearPropertyErrors(propertyName);

            switch (propertyName)
            {
                case nameof(Title):                    var titleValidation = _validationService?.ValidateString(value?.ToString(), 3, 100, true);
                    if (titleValidation != null && !titleValidation.IsValid)
                    {
                        foreach (var error in titleValidation.Errors)
                        {
                            AddPropertyError(propertyName, error);
                        }
                    }
                    break;

                case nameof(Description):
                    if (!string.IsNullOrEmpty(value?.ToString()))
                    {                        var descriptionValidation = _validationService?.ValidateString(value.ToString(), 0, 1000, false);
                        if (descriptionValidation != null && !descriptionValidation.IsValid)
                        {
                            foreach (var error in descriptionValidation.Errors)
                            {
                                AddPropertyError(propertyName, error);
                            }
                        }
                    }
                    break;                case nameof(TimeLimit):
                    if (double.TryParse(value?.ToString(), out double timeLimit))
                    {
                        var timeLimitValidation = _validationService?.ValidateNumber(timeLimit, 1, 480);
                        if (timeLimitValidation != null && !timeLimitValidation.IsValid)
                        {
                            foreach (var error in timeLimitValidation.Errors)
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
            ValidateProperty(Title, nameof(Title));
            ValidateProperty(Description, nameof(Description));
            ValidateProperty(TimeLimit, nameof(TimeLimit));

            // Additional business logic validation
            if (string.IsNullOrWhiteSpace(Title))
            {
                AddPropertyError(nameof(Title), "Test title is required");
            }

            if (TimeLimit < 1)
            {
                AddPropertyError(nameof(TimeLimit), "Time limit must be at least 1 minute");
            }
            else if (TimeLimit > 480)
            {
                AddPropertyError(nameof(TimeLimit), "Time limit cannot exceed 480 minutes (8 hours)");
            }

            await Task.CompletedTask;
        }        public bool IsFormValid()
        {
            ValidateForm().Wait();
            return !HasAnyErrors;
        }
    }
}