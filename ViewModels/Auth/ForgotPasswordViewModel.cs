using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels
{
    public partial class ForgotPasswordViewModel : AuthenticationViewModel
    {
        [ObservableProperty]
        private string _email = string.Empty;

        public ICommand SendResetInstructionsCommand { get; }
        public ICommand BackToSignInCommand { get; set; } = null!;

        public event EventHandler? ResetEmailSent;

        public ForgotPasswordViewModel() : base()
        {
            SendResetInstructionsCommand = new AsyncRelayCommand(SendResetInstructionsAsync);
        }

        partial void OnEmailChanged(string value)
        {
            ValidateProperty(value);
        }

        protected new void ValidateProperty<T>(T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null) return;

            // Clear existing errors for this property
            ClearPropertyErrors(propertyName);

            switch (propertyName)
            {                case nameof(Email):
                    if (value is string email)
                    {
                        if (string.IsNullOrWhiteSpace(email))
                        {
                            AddPropertyError(propertyName, "Email is required");
                        }
                        else if (_validationService?.IsValidEmail(email) == false)
                        {
                            AddPropertyError(propertyName, "Please enter a valid email address");
                        }
                    }
                    break;
            }
        }        private async Task SendResetInstructionsAsync()
        {            // Validate email before sending reset instructions
            ValidateProperty(Email, nameof(Email));
            if (HasAnyErrors) return;

            IsLoading = true;
            ClearError();

            try
            {
                var success = await _authService.SendPasswordResetAsync(Email);

                if (success)
                {
                    ShowSuccess();
                    ResetEmailSent?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ShowError("Failed to send reset instructions. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }        }
    }
}