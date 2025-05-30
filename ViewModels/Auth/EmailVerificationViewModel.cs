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

        [ObservableProperty]
        private string _descriptionText = "Please enter the verification code sent to your email address";

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
                var resendType = IsPasswordReset ? "recovery" : "signup";
                var success = await _authService.ResendVerificationCodeAsync(Email, resendType);

                if (success)
                {
                    var message = IsPasswordReset
                        ? "Password reset email resent successfully! Please check your email."
                        : "Verification code resent successfully! Please check your email.";
                    ShowSuccess(message);
                }
                else
                {
                    ShowError("Failed to resend verification code. Please try again.");
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

        public void SetupForPasswordReset(string email)
        {
            Email = email;
            IsPasswordReset = true;
            DescriptionText = "Please enter the verification code sent to your email to reset your password";
        }

        public void SetupForSignupVerification(string email)
        {
            Email = email;
            IsPasswordReset = false;
            DescriptionText = "Please enter the verification code sent to your email to verify your account";
        }
    }
}