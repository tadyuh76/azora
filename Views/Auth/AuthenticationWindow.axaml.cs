using Avalonia.Controls;
using AvaloniaAzora.ViewModels;
using CommunityToolkit.Mvvm.Input;
using System;

namespace AvaloniaAzora.Views.Auth
{
    public partial class AuthenticationWindow : Window
    {
        private SignInViewModel _signInViewModel = null!;
        private SignUpViewModel _signUpViewModel = null!;
        private ForgotPasswordViewModel _forgotPasswordViewModel = null!;
        private EmailVerificationViewModel _emailVerificationViewModel = null!;

        public event EventHandler? AuthenticationSuccessful;

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
            _signInViewModel.SignInSuccessful += OnSignInSuccessful;

            _signUpViewModel = new SignUpViewModel();
            _signUpViewModel.SignInCommand = new RelayCommand(() => ShowView("SignIn"));
            _signUpViewModel.SignUpSuccessful += OnSignUpSuccessful;

            _forgotPasswordViewModel = new ForgotPasswordViewModel();
            _forgotPasswordViewModel.BackToSignInCommand = new RelayCommand(() => ShowView("SignIn"));
            _forgotPasswordViewModel.ResetEmailSent += OnResetEmailSent;

            _emailVerificationViewModel = new EmailVerificationViewModel();
            _emailVerificationViewModel.BackToSignInCommand = new RelayCommand(() => ShowView("SignIn"));
            _emailVerificationViewModel.VerificationSuccessful += OnVerificationSuccessful;

            // Set DataContext for each view
            this.FindControl<Control>("SignInView")!.DataContext = _signInViewModel;
            this.FindControl<Control>("SignUpView")!.DataContext = _signUpViewModel;
            this.FindControl<Control>("ForgotPasswordView")!.DataContext = _forgotPasswordViewModel;
            this.FindControl<Control>("EmailVerificationView")!.DataContext = _emailVerificationViewModel;
        }

        private void OnSignInSuccessful(object? sender, EventArgs e)
        {
            // User successfully signed in - notify the app
            AuthenticationSuccessful?.Invoke(this, EventArgs.Empty);
        }

        private void OnSignUpSuccessful(object? sender, EventArgs e)
        {
            // User successfully signed up - show email verification
            _emailVerificationViewModel.SetupForSignupVerification(_signUpViewModel.Email);
            ShowView("EmailVerification");
        }

        private void OnResetEmailSent(object? sender, EventArgs e)
        {
            // Password reset email sent - show email verification for reset
            _emailVerificationViewModel.SetupForPasswordReset(_forgotPasswordViewModel.Email);
            ShowView("EmailVerification");
        }

        private void OnVerificationSuccessful(object? sender, EventArgs e)
        {
            // Email verified successfully - go back to sign in
            ShowView("SignIn");

            // Show a success message on the sign in page
            _signInViewModel.ClearMessages();
            _signInViewModel.SetSuccessMessage("Email verified successfully! You can now sign in.");
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