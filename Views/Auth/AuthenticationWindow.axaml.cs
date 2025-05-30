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
        private ChangePasswordViewModel _changePasswordViewModel = null!;

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

            _changePasswordViewModel = new ChangePasswordViewModel();
            _changePasswordViewModel.BackToSignInCommand = new RelayCommand(() => ShowView("SignIn"));
            _changePasswordViewModel.PasswordChanged += OnPasswordChanged;

            // Set DataContext for each view
            this.FindControl<Control>("SignInView")!.DataContext = _signInViewModel;
            this.FindControl<Control>("SignUpView")!.DataContext = _signUpViewModel;
            this.FindControl<Control>("ForgotPasswordView")!.DataContext = _forgotPasswordViewModel;
            this.FindControl<Control>("EmailVerificationView")!.DataContext = _emailVerificationViewModel;
            this.FindControl<Control>("ChangePasswordView")!.DataContext = _changePasswordViewModel;
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
            if (_emailVerificationViewModel.IsPasswordReset)
            {
                // Password reset OTP verified - show change password view
                _changePasswordViewModel.Email = _emailVerificationViewModel.Email;
                ShowView("ChangePassword");
            }
            else
            {
                // Email verified successfully for signup - go back to sign in
                ShowView("SignIn");
                _signInViewModel.ClearMessages();
                _signInViewModel.SetSuccessMessage("Email verified successfully! You can now sign in.");
            }
        }

        private void OnPasswordChanged(object? sender, EventArgs e)
        {
            // Password changed successfully - go back to sign in
            ShowView("SignIn");
            _signInViewModel.ClearMessages();
            _signInViewModel.SetSuccessMessage("Password changed successfully! You can now sign in with your new password.");
        }

        private void ShowView(string viewName)
        {
            // Hide all views first
            this.FindControl<Control>("SignInView")!.IsVisible = false;
            this.FindControl<Control>("SignUpView")!.IsVisible = false;
            this.FindControl<Control>("ForgotPasswordView")!.IsVisible = false;
            this.FindControl<Control>("EmailVerificationView")!.IsVisible = false;
            this.FindControl<Control>("ChangePasswordView")!.IsVisible = false;

            // Show the requested view
            var viewControl = viewName switch
            {
                "SignIn" => this.FindControl<Control>("SignInView"),
                "SignUp" => this.FindControl<Control>("SignUpView"),
                "ForgotPassword" => this.FindControl<Control>("ForgotPasswordView"),
                "EmailVerification" => this.FindControl<Control>("EmailVerificationView"),
                "ChangePassword" => this.FindControl<Control>("ChangePasswordView"),
                _ => this.FindControl<Control>("SignInView")
            };

            if (viewControl != null)
            {
                viewControl.IsVisible = true;
            }
        }
    }
}