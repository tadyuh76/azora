using Avalonia.Controls;
using AvaloniaAzora.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaAzora.Views.Auth
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
            this.FindControl<Control>("SignInView")!.DataContext = _signInViewModel;
            this.FindControl<Control>("SignUpView")!.DataContext = _signUpViewModel;
            this.FindControl<Control>("ForgotPasswordView")!.DataContext = _forgotPasswordViewModel;
            this.FindControl<Control>("EmailVerificationView")!.DataContext = _emailVerificationViewModel;
        }

        private void ShowView(string viewName)
        {
            // Hide all views first
            this.FindControl<Control>("SignInView")!.IsVisible = false;
            this.FindControl<Control>("SignUpView")!.IsVisible = false;
            this.FindControl<Control>("ForgotPasswordView")!.IsVisible = false;
            this.FindControl<Control>("EmailVerificationView")!.IsVisible = false;

            // Show the requested view
            switch (viewName)
            {
                case "SignIn":
                    this.FindControl<Control>("SignInView")!.IsVisible = true;
                    break;
                case "SignUp":
                    this.FindControl<Control>("SignUpView")!.IsVisible = true;
                    break;
                case "ForgotPassword":
                    this.FindControl<Control>("ForgotPasswordView")!.IsVisible = true;
                    break;
                case "EmailVerification":
                    this.FindControl<Control>("EmailVerificationView")!.IsVisible = true;
                    break;
                default:
                    this.FindControl<Control>("SignInView")!.IsVisible = true; // Default to SignIn
                    break;
            }
        }
    }
}