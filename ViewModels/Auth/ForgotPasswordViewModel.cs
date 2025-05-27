using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels
{
    public partial class ForgotPasswordViewModel : AuthenticationViewModel
    {
        public ICommand SendResetInstructionsCommand { get; }
        public ICommand BackToSignInCommand { get; set; }

        public ForgotPasswordViewModel()
        {
            SendResetInstructionsCommand = new RelayCommand(SendResetInstructions);
            BackToSignInCommand = new RelayCommand(BackToSignIn);
        }

        private void SendResetInstructions()
        {
            // TODO: Implement password reset logic
        }

        private void BackToSignIn()
        {
            // TODO: Navigate back to sign in page
        }
    }
}