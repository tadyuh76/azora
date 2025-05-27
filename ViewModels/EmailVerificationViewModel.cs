using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

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
        public ICommand BackToSignInCommand { get; set; }

        public EmailVerificationViewModel()
        {
            VerifyCommand = new RelayCommand(Verify);
            ResendCodeCommand = new RelayCommand(ResendCode);
            BackToSignInCommand = new RelayCommand(BackToSignIn);
        }

        private void Verify()
        {
            // TODO: Implement verification logic
        }

        private void ResendCode()
        {
            // TODO: Implement resend code logic
        }

        private void BackToSignIn()
        {
            // TODO: Navigate back to sign in page
        }
    }
}