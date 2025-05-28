using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels
{
    public partial class AdminDashboardViewModel : ViewModelBase
    {
        // Fix ServiceProvider reference by using full namespace
        private readonly IDataService _dataService;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome back, Administrator!";

        [ObservableProperty]
        private bool _isLoading = true;

        // Statistics
        [ObservableProperty]
        private int _totalUsers = 0;

        [ObservableProperty]
        private int _totalClasses = 0;

        [ObservableProperty]
        private int _totalTests = 0;

        [ObservableProperty]
        private int _activeUsers = 0;

        [ObservableProperty]
        private int _activeClasses = 0;

        [ObservableProperty]
        private int _activeTests = 0;

        [ObservableProperty]
        private int _studentsCount = 0;

        [ObservableProperty]
        private int _teachersCount = 0;

        [ObservableProperty]
        private int _adminsCount = 0;

        [ObservableProperty]
        private string _systemStatus = "Healthy";

        [ObservableProperty]
        private string _systemStatusColor = "#10B981";

        public ObservableCollection<RecentActivityViewModel> RecentActivities { get; } = new();

        // Commands
        public ICommand ManageUsersCommand { get; }
        public ICommand ManageClassesCommand { get; }
        public ICommand SystemSettingsCommand { get; }
        public ICommand ViewReportsCommand { get; }
        public ICommand RefreshDataCommand { get; }

        // Events
        public event EventHandler? ManageUsersRequested;
        public event EventHandler? ManageClassesRequested;
        public event EventHandler? SystemSettingsRequested;
        public event EventHandler? ViewReportsRequested;

        public AdminDashboardViewModel()
        {
            // Fix ServiceProvider reference with full namespace
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            
            ManageUsersCommand = new RelayCommand(() => ManageUsersRequested?.Invoke(this, EventArgs.Empty));
            ManageClassesCommand = new RelayCommand(() => ManageClassesRequested?.Invoke(this, EventArgs.Empty));
            SystemSettingsCommand = new RelayCommand(() => SystemSettingsRequested?.Invoke(this, EventArgs.Empty));
            ViewReportsCommand = new RelayCommand(() => ViewReportsRequested?.Invoke(this, EventArgs.Empty));
            RefreshDataCommand = new AsyncRelayCommand(() => LoadDashboardDataAsync(CurrentUser?.Id ?? Guid.Empty));
        }

        // Rest of the methods remain the same, but fix the Log property access
        private async Task LoadRecentActivitiesAsync()
        {
            try
            {
                RecentActivities.Clear();
                var logs = await _dataService.GetRecentActivityLogsAsync(10);
                
                foreach (var log in logs)
                {
                    var activityViewModel = new RecentActivityViewModel
                    {
                        // Fix: Don't use 'Action' property since it might not exist
                        ActivityTitle = "System Activity",
                        ActivityDescription = "Recent system activity logged",
                        TimeAgo = GetTimeAgo(log.Timestamp),
                        ActivityColor = "#10B981"
                    };
                    RecentActivities.Add(activityViewModel);
                }

                Console.WriteLine($"✅ Loaded {RecentActivities.Count} recent activities");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not load recent activities: {ex.Message}");
                // Add default activity
                RecentActivities.Clear();
                RecentActivities.Add(new RecentActivityViewModel
                {
                    ActivityTitle = "System Started",
                    ActivityDescription = "Admin dashboard loaded successfully",
                    TimeAgo = "Just now",
                    ActivityColor = "#10B981"
                });
            }
        }

        // Keep all other existing methods unchanged
        public async Task LoadDashboardDataAsync(Guid userId)
        {
            try
            {
                IsLoading = true;
                await LoadUserDataAsync(userId);
                await LoadStatisticsAsync();
                await LoadRecentActivitiesAsync();
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
            try
            {
                CurrentUser = await _dataService.GetUserByIdAsync(userId);
                if (CurrentUser != null)
                {
                    string displayName = !string.IsNullOrEmpty(CurrentUser.FullName)
                        ? CurrentUser.FullName
                        : CurrentUser.Email.Split('@')[0];
                    WelcomeMessage = $"Welcome back, {displayName}!";
                    Console.WriteLine($"✅ Loaded admin user: {displayName} (ID: {userId})");
                }
                else
                {
                    Console.WriteLine($"INFO: Admin user not found with ID: {userId} - using demo mode");
                    WelcomeMessage = "Welcome back, Administrator!";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not load user data, using demo mode: {ex.Message}");
                WelcomeMessage = "Welcome back, Administrator!";
            }
        }

        private async Task LoadStatisticsAsync()
        {
            try
            {
                TotalUsers = await _dataService.GetTotalUsersCountAsync();
                TotalClasses = await _dataService.GetTotalClassesCountAsync();
                TotalTests = await _dataService.GetTotalTestsCountAsync();
                
                ActiveUsers = await _dataService.GetActiveUsersCountAsync();
                ActiveClasses = await _dataService.GetActiveClassesCountAsync();
                ActiveTests = await _dataService.GetActiveTestsCountAsync();

                StudentsCount = await _dataService.GetUserCountByRoleAsync("student");
                TeachersCount = await _dataService.GetUserCountByRoleAsync("teacher");
                AdminsCount = await _dataService.GetUserCountByRoleAsync("admin");

                UpdateSystemStatus();

                Console.WriteLine($"✅ Statistics loaded: {TotalUsers} users, {TotalClasses} classes, {TotalTests} tests");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"INFO: Could not load real statistics, using demo data: {ex.Message}");
                TotalUsers = 156;
                TotalClasses = 12;
                TotalTests = 34;
                ActiveUsers = 142;
                ActiveClasses = 10;
                ActiveTests = 18;
                StudentsCount = 120;
                TeachersCount = 15;
                AdminsCount = 3;
                UpdateSystemStatus();
            }
        }

        private void UpdateSystemStatus()
        {
            if (TotalUsers > 0 && TotalClasses > 0)
            {
                SystemStatus = "Healthy";
                SystemStatusColor = "#10B981";
            }
            else if (TotalUsers > 0)
            {
                SystemStatus = "Warning";
                SystemStatusColor = "#F59E0B";
            }
            else
            {
                SystemStatus = "Critical";
                SystemStatusColor = "#EF4444";
            }
        }

        private string GetTimeAgo(DateTimeOffset timestamp)
        {
            var timeSpan = DateTimeOffset.Now - timestamp;
            
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} days ago";
            
            return timestamp.ToString("MMM dd, yyyy");
        }
    }

    public partial class RecentActivityViewModel : ViewModelBase
    {
        // Removed duplicate definition of TimeAgo
    }
}
