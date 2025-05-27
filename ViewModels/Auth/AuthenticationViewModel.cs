using CommunityToolkit.Mvvm.ComponentModel;
using AvaloniaAzora.Services;

namespace AvaloniaAzora.ViewModels
{
    public partial class AuthenticationViewModel : ViewModelBase
    {
        protected readonly IAuthenticationService _authService;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private string _successMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private bool _isSuccess = false;

        public AuthenticationViewModel()
        {
            _authService = ServiceProvider.GetService<IAuthenticationService>();
        }

        protected void ClearError()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }

        protected void ShowError(string message)
        {
            ErrorMessage = message;
            SuccessMessage = string.Empty;
            IsSuccess = false;
        }

        protected void ShowSuccess(string message = "")
        {
            ErrorMessage = string.Empty;
            SuccessMessage = message;
            IsSuccess = true;
        }

        public void ClearMessages()
        {
            ClearError();
        }

        public void SetSuccessMessage(string message)
        {
            ShowSuccess(message);
        }
    }
}