using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Supabase.Gotrue;

namespace AvaloniaAzora.ViewModels
{
    public partial class EmailVerificationViewModel : AuthenticationViewModel
    {
        [ObservableProperty]
        private string _verificationCode = string.Empty;

        [ObservableProperty]
        private bool _isPasswordReset = false;

        public ICommand VerifyCommand { get; }
        public ICommand ResendCodeCommand { get; }
        public ICommand BackToSignInCommand { get; set; } = null!;

        public event EventHandler? VerificationSuccessful;

        public EmailVerificationViewModel() : base()
        {
            VerifyCommand = new AsyncRelayCommand(VerifyAsync);
            ResendCodeCommand = new AsyncRelayCommand(ResendCodeAsync);
        }

        private async Task VerifyAsync()
        {
            if (string.IsNullOrWhiteSpace(VerificationCode))
            {
                ShowError("Please enter the verification code.");
                return;
            }

            if (VerificationCode.Length != 6)
            {
                ShowError("Verification code must be 6 digits.");
                return;
            }

            IsLoading = true;
            ClearError();

            try
            {
                var otpType = IsPasswordReset ? "recovery" : "signup";
                var session = await _authService.VerifyOtpAsync(Email, VerificationCode, otpType);

                if (session != null)
                {
                    ShowSuccess();
                    VerificationSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ShowError("Invalid verification code. Please try again.");
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

        private async Task ResendCodeAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ShowError("Email address is required to resend code.");
                return;
            }

            IsLoading = true;
            ClearError();

            try
            {
                if (IsPasswordReset)
                {
                    await _authService.SendPasswordResetAsync(Email);
                }
                else
                {
                    // For signup verification, we'd typically need to trigger a resend
                    // This might require additional Supabase setup
                    ShowError("Please contact support to resend verification code.");
                    return;
                }

                ShowSuccess();
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

        public void SetupForPasswordReset(string email)
        {
            Email = email;
            IsPasswordReset = true;
        }

        public void SetupForSignupVerification(string email)
        {
            Email = email;
            IsPasswordReset = false;
        }
    }
}