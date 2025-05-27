using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels
{
    public partial class SignInViewModel : AuthenticationViewModel
    {
        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _rememberMe = false;

        public ICommand SignInCommand { get; }
        public ICommand ForgotPasswordCommand { get; set; }
        public ICommand SignUpCommand { get; set; }

        public SignInViewModel()
        {
            SignInCommand = new RelayCommand(SignIn);
            ForgotPasswordCommand = new RelayCommand(ForgotPassword);
            SignUpCommand = new RelayCommand(SignUp);
        }

        private void SignIn()
        {
            // TODO: Implement sign in logic
        }

        private void ForgotPassword()
        {
            // TODO: Navigate to forgot password page
        }

        private void SignUp()
        {
            // TODO: Navigate to sign up page
        }
    }
}