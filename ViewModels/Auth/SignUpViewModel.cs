using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        [ObservableProperty]
        private bool _isPasswordVisible = false;

        [ObservableProperty]
        private bool _isConfirmPasswordVisible = false;

        public ICommand CreateAccountCommand { get; }
        public ICommand SignInCommand { get; set; } = null!;
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand ToggleConfirmPasswordVisibilityCommand { get; }

        public event EventHandler? SignUpSuccessful;

        public SignUpViewModel() : base()
        {
            CreateAccountCommand = new AsyncRelayCommand(CreateAccountAsync);
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
            ToggleConfirmPasswordVisibilityCommand = new RelayCommand(ToggleConfirmPasswordVisibility);
        }

        private async Task CreateAccountAsync()
        {
            if (!ValidateInput())
                return;

            IsLoading = true;
            ClearError();

            try
            {
                var session = await _authService.SignUpAsync(Email, Password, FullName);

                if (session != null)
                {
                    ShowSuccess();
                    SignUpSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ShowError("Sign up failed. Please try again.");
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
        }        private bool ValidateInput()
        {
            ClearAllErrors();
            bool isValid = true;

            // Validate Full Name
            var fullNameResult = _validationService?.ValidateString(FullName, minLength: 2, maxLength: 100, required: true, propertyName: "Full Name");
            if (fullNameResult != null && !fullNameResult.IsValid)
            {
                ShowError(fullNameResult.FirstError);
                isValid = false;
            }

            // Validate Email
            if (string.IsNullOrWhiteSpace(Email))
            {
                ShowError("Please enter your email address.");
                isValid = false;
            }
            else if (_validationService?.IsValidEmail(Email) == false)
            {
                ShowError("Please enter a valid email address.");
                isValid = false;
            }

            // Validate Password
            if (string.IsNullOrWhiteSpace(Password))
            {
                ShowError("Please enter a password.");
                isValid = false;
            }
            else if (_validationService?.IsValidPassword(Password) == false)
            {
                ShowError("Password must be between 6 and 128 characters long.");
                isValid = false;
            }

            // Validate Password Confirmation
            if (Password != ConfirmPassword)
            {
                ShowError("Passwords do not match.");
                isValid = false;
            }

            // Validate Terms Agreement
            if (!AgreeToTerms)
            {
                ShowError("Please agree to the Terms & Conditions and Privacy Policy.");
                isValid = false;
            }

            return isValid;
        }

        // Keep legacy method for backward compatibility, but use validation service internally
        private static bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }

        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private void ToggleConfirmPasswordVisibility()
        {
            IsConfirmPasswordVisible = !IsConfirmPasswordVisible;
        }
    }
}