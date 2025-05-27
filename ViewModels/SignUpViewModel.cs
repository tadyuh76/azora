using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels
{
    public partial class SignUpViewModel : AuthenticationViewModel
    {
        [ObservableProperty]
        private string _fullName = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        private bool _agreeToTerms = false;

        public ICommand CreateAccountCommand { get; }
        public ICommand SignInCommand { get; set; }

        public SignUpViewModel()
        {
            CreateAccountCommand = new RelayCommand(CreateAccount);
            SignInCommand = new RelayCommand(SignIn);
        }

        private void CreateAccount()
        {
            // TODO: Implement account creation logic
        }

        private void SignIn()
        {
            // TODO: Navigate to sign in page
        }
    }
}