using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaAzora.ViewModels
{
    public partial class AuthenticationViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;
    }
}