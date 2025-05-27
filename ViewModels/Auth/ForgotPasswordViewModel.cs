using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels
{
    public partial class ForgotPasswordViewModel : AuthenticationViewModel
    {
        public ICommand SendResetInstructionsCommand { get; }
        public ICommand BackToSignInCommand { get; set; } = null!;

        public event EventHandler? ResetEmailSent;

        public ForgotPasswordViewModel() : base()
        {
            SendResetInstructionsCommand = new AsyncRelayCommand(SendResetInstructionsAsync);
        }

        private async Task SendResetInstructionsAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ShowError("Please enter your email address.");
                return;
            }

            if (!IsValidEmail(Email))
            {
                ShowError("Please enter a valid email address.");
                return;
            }

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
            }
        }

        private static bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }
    }
}