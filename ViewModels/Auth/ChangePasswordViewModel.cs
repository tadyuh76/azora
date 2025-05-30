using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels
{
    public partial class ChangePasswordViewModel : AuthenticationViewModel
    {
        [ObservableProperty]
        private string _newPassword = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        private bool _isNewPasswordVisible = false;

        [ObservableProperty]
        private bool _isConfirmPasswordVisible = false;

        public ICommand ChangePasswordCommand { get; }
        public ICommand ToggleNewPasswordVisibilityCommand { get; }
        public ICommand ToggleConfirmPasswordVisibilityCommand { get; }
        public ICommand BackToSignInCommand { get; set; } = null!;

        public event EventHandler? PasswordChanged;

        public ChangePasswordViewModel() : base()
        {
            ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync);
            ToggleNewPasswordVisibilityCommand = new RelayCommand(ToggleNewPasswordVisibility);
            ToggleConfirmPasswordVisibilityCommand = new RelayCommand(ToggleConfirmPasswordVisibility);
        }

        private async Task ChangePasswordAsync()
        {
            if (!ValidateInput())
                return;

            IsLoading = true;
            ClearError();

            try
            {
                // Get current session after OTP verification
                var session = await _authService.GetCurrentSessionAsync();
                if (session == null)
                {
                    ShowError("Session expired. Please restart the password reset process.");
                    return;
                }

                // Update the password using Supabase's updateUser method
                // Note: In Supabase, after OTP verification for password reset, 
                // the user is automatically signed in with a temporary session
                var success = await _authService.UpdatePasswordAsync(NewPassword);

                if (success)
                {
                    ShowSuccess("Password updated successfully!");
                    PasswordChanged?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ShowError("Failed to update password. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ShowError("Please enter a new password.");
                return false;
            }

            if (NewPassword.Length < 6)
            {
                ShowError("Password must be at least 6 characters long.");
                return false;
            }

            if (NewPassword != ConfirmPassword)
            {
                ShowError("Passwords do not match.");
                return false;
            }

            return true;
        }

        private void ToggleNewPasswordVisibility()
        {
            IsNewPasswordVisible = !IsNewPasswordVisible;
        }

        private void ToggleConfirmPasswordVisibility()
        {
            IsConfirmPasswordVisible = !IsConfirmPasswordVisible;
        }
    }
}