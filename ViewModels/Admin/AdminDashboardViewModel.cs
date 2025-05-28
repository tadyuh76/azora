using CommunityToolkit.Mvvm.ComponentModel;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaAzora.ViewModels
{
    public partial class AdminDashboardViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome back, Administrator!";

        [ObservableProperty]
        private bool _isLoading = true;

        public AdminDashboardViewModel()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
        }

        public async Task LoadDashboardDataAsync(Guid userId)
        {
            try
            {
                IsLoading = true;
                await LoadUserDataAsync(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error loading admin dashboard: {ex.Message}");
                WelcomeMessage = "Welcome back, Administrator!";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadUserDataAsync(Guid userId)
        {
            // Load current user
            CurrentUser = await _dataService.GetUserByIdAsync(userId);
            if (CurrentUser != null)
            {
                // Use full_name if available, otherwise use email prefix
                string displayName = !string.IsNullOrEmpty(CurrentUser.FullName)
                    ? CurrentUser.FullName
                    : CurrentUser.Email.Split('@')[0];

                WelcomeMessage = $"Welcome back, {displayName}!";
                Console.WriteLine($"✅ Loaded admin user: {displayName} (ID: {userId})");
            }
            else
            {
                Console.WriteLine($"⚠️ Admin user not found with ID: {userId}");
                WelcomeMessage = "Welcome back, Administrator!";
            }
        }
    }
}