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

        public ICommand CreateAccountCommand { get; }
        public ICommand SignInCommand { get; set; } = null!;

        public event EventHandler? SignUpSuccessful;

        public SignUpViewModel() : base()
        {
            CreateAccountCommand = new AsyncRelayCommand(CreateAccountAsync);
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
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                ShowError("Please enter your full name.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ShowError("Please enter your email address.");
                return false;
            }

            if (!IsValidEmail(Email))
            {
                ShowError("Please enter a valid email address.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ShowError("Please enter a password.");
                return false;
            }

            if (Password.Length < 6)
            {
                ShowError("Password must be at least 6 characters long.");
                return false;
            }

            if (Password != ConfirmPassword)
            {
                ShowError("Passwords do not match.");
                return false;
            }

            if (!AgreeToTerms)
            {
                ShowError("Please agree to the Terms & Conditions and Privacy Policy.");
                return false;
            }

            return true;
        }

        private static bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return emailRegex.IsMatch(email);
        }
    }
}