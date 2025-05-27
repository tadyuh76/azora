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
        }

        protected void ShowError(string message)
        {
            ErrorMessage = message;
            IsSuccess = false;
        }

        protected void ShowSuccess()
        {
            ErrorMessage = string.Empty;
            IsSuccess = true;
        }
    }
}