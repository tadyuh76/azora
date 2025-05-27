using Avalonia.Controls;
using AvaloniaAzora.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaAzora.Views
{
    public partial class AuthenticationWindow : Window
    {
        private SignInViewModel _signInViewModel = null!;
        private SignUpViewModel _signUpViewModel = null!;
        private ForgotPasswordViewModel _forgotPasswordViewModel = null!;
        private EmailVerificationViewModel _emailVerificationViewModel = null!;

        public AuthenticationWindow()
        {
            InitializeComponent();
            SetupViewModels();
        }

        private void SetupViewModels()
        {
            // Create ViewModels with navigation commands
            _signInViewModel = new SignInViewModel();
            _signInViewModel.ForgotPasswordCommand = new RelayCommand(() => ShowView("ForgotPassword"));
            _signInViewModel.SignUpCommand = new RelayCommand(() => ShowView("SignUp"));

            _signUpViewModel = new SignUpViewModel();
            _signUpViewModel.SignInCommand = new RelayCommand(() => ShowView("SignIn"));

            _forgotPasswordViewModel = new ForgotPasswordViewModel();
            _forgotPasswordViewModel.BackToSignInCommand = new RelayCommand(() => ShowView("SignIn"));

            _emailVerificationViewModel = new EmailVerificationViewModel();
            _emailVerificationViewModel.BackToSignInCommand = new RelayCommand(() => ShowView("SignIn"));

            // Set DataContext for each view
            SignInView.DataContext = _signInViewModel;
            SignUpView.DataContext = _signUpViewModel;
            ForgotPasswordView.DataContext = _forgotPasswordViewModel;
            EmailVerificationView.DataContext = _emailVerificationViewModel;
        }

        private void ShowView(string viewName)
        {
            // Hide all views first
            SignInView.IsVisible = false;
            SignUpView.IsVisible = false;
            ForgotPasswordView.IsVisible = false;
            EmailVerificationView.IsVisible = false;

            // Show the requested view
            switch (viewName)
            {
                case "SignIn":
                    SignInView.IsVisible = true;
                    break;
                case "SignUp":
                    SignUpView.IsVisible = true;
                    break;
                case "ForgotPassword":
                    ForgotPasswordView.IsVisible = true;
                    break;
                case "EmailVerification":
                    EmailVerificationView.IsVisible = true;
                    break;
                default:
                    SignInView.IsVisible = true; // Default to SignIn
                    break;
            }
        }
    }
}