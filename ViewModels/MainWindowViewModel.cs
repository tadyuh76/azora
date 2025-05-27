using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Services;
using System.Threading.Tasks;
using System.Windows.Input;
using System;

namespace AvaloniaAzora.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private readonly IAuthenticationService _authService;

        [ObservableProperty]
        private string _userEmail = "Loading...";

        [ObservableProperty]
        private string _userName = "Loading...";

        public ICommand SignOutCommand { get; }
        public ICommand RefreshUserInfoCommand { get; }

        public event EventHandler? SignOutRequested;

        public MainWindowViewModel()
        {
            _authService = ServiceProvider.GetService<IAuthenticationService>();

            SignOutCommand = new AsyncRelayCommand(SignOutAsync);
            RefreshUserInfoCommand = new AsyncRelayCommand(RefreshUserInfoAsync);

            // Load user info when the ViewModel is created
            _ = RefreshUserInfoAsync();
        }

        private async Task RefreshUserInfoAsync()
        {
            try
            {
                var session = await _authService.GetCurrentSessionAsync();
                var user = _authService.GetCurrentUser();

                if (user != null)
                {
                    UserEmail = user.Email ?? "Unknown";

                    // Try to get the full name from user metadata
                    if (user.UserMetadata?.ContainsKey("full_name") == true)
                    {
                        UserName = user.UserMetadata["full_name"]?.ToString() ?? "Unknown";
                    }
                    else
                    {
                        UserName = "Unknown";
                    }
                }
                else
                {
                    UserEmail = "Not signed in";
                    UserName = "Not signed in";
                }
            }
            catch (Exception)
            {
                UserEmail = "Error loading user info";
                UserName = "Error loading user info";
            }
        }

        private async Task SignOutAsync()
        {
            try
            {
                await _authService.SignOutAsync();
                SignOutRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception)
            {
                // Handle sign out error if needed
            }
        }
    }
}