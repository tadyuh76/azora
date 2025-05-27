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

        public ICommand SignInCommand { get; }
        public ICommand ForgotPasswordCommand { get; set; } = null!;
        public ICommand SignUpCommand { get; set; } = null!;

        public event EventHandler? SignInSuccessful;

        public SignInViewModel() : base()
        {
            SignInCommand = new AsyncRelayCommand(SignInAsync);
        }

        private async Task SignInAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ShowError("Please enter both email and password.");
                return;
            }

            IsLoading = true;
            ClearError();

            try
            {
                var session = await _authService.SignInAsync(Email, Password);

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
    }
}