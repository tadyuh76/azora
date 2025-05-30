using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels
{
    public partial class SignInViewModel : AuthenticationViewModel
    {
        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _rememberMe = false;

        [ObservableProperty]
        private bool _isPasswordVisible = false;

        public ICommand SignInCommand { get; }
        public ICommand ForgotPasswordCommand { get; set; } = null!;
        public ICommand SignUpCommand { get; set; } = null!;
        public ICommand TogglePasswordVisibilityCommand { get; }

        public event EventHandler? SignInSuccessful;

        public SignInViewModel() : base()
        {
            SignInCommand = new AsyncRelayCommand(SignInAsync);
            TogglePasswordVisibilityCommand = new RelayCommand(TogglePasswordVisibility);
        }

        private async Task SignInAsync()
        {
            if (!ValidateInput())
                return;

            IsLoading = true;
            ClearError(); try
            {
                var session = await _authService.SignInAsync(Email, Password, RememberMe);

                if (session != null)
                {
                    ShowSuccess();
                    SignInSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    ShowError("Sign in failed. Please check your credentials.");
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

        private void TogglePasswordVisibility()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        private bool ValidateInput()
        {
            ClearAllErrors();
            bool isValid = true;

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
                ShowError("Please enter your password.");
                isValid = false;
            }

            return isValid;
        }
    }
}