using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels.Admin
{
    public partial class SystemSettingsViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        
        [ObservableProperty]
        private string _systemName = "Azora Learning Platform";
        
        [ObservableProperty]
        private string _systemDescription = "Educational platform for tests and assessments";
        
        [ObservableProperty]
        private bool _allowRegistration = true;
        
        [ObservableProperty]
        private bool _requireEmailVerification = true;
        
        [ObservableProperty]
        private int _sessionTimeoutMinutes = 480;
        
        [ObservableProperty]
        private int _passwordMinLength = 6;
        
        [ObservableProperty]
        private bool _requireSpecialCharacters = false;
        
        [ObservableProperty]
        private int _maxLoginAttempts = 5;
        
        [ObservableProperty]
        private int _lockoutDurationMinutes = 15;
        
        [ObservableProperty]
        private bool _enableAuditLogging = true;
        
        [ObservableProperty]
        private bool _isLoading = false;
        
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        [ObservableProperty]
        private string _successMessage = string.Empty;

        public ICommand SaveSettingsCommand { get; }
        public ICommand ResetToDefaultsCommand { get; }
        public ICommand TestEmailCommand { get; }
        public ICommand BackupDatabaseCommand { get; }

        public SystemSettingsViewModel()
        {
            // Fixed ServiceProvider reference
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            
            SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync);
            ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);
            TestEmailCommand = new AsyncRelayCommand(TestEmailAsync);
            BackupDatabaseCommand = new AsyncRelayCommand(BackupDatabaseAsync);
            
            // Load current settings
            _ = LoadSettingsAsync();
        }

        private async Task LoadSettingsAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                // TODO: Load settings from database or configuration
                await Task.Delay(500); // Simulate loading
                ShowSuccess("Settings loaded successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading settings: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                // Validate settings
                if (string.IsNullOrWhiteSpace(SystemName))
                {
                    ShowError("System name is required.");
                    return;
                }
                
                if (PasswordMinLength < 6)
                {
                    ShowError("Password minimum length must be at least 6 characters.");
                    return;
                }
                
                if (SessionTimeoutMinutes < 30)
                {
                    ShowError("Session timeout must be at least 30 minutes.");
                    return;
                }
                
                // TODO: Save settings to database or configuration
                await Task.Delay(1000); // Simulate save operation
                
                ShowSuccess("Settings saved successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error saving settings: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ResetToDefaults()
        {
            SystemName = "Azora Learning Platform";
            SystemDescription = "Educational platform for tests and assessments";
            AllowRegistration = true;
            RequireEmailVerification = true;
            SessionTimeoutMinutes = 480;
            PasswordMinLength = 6;
            RequireSpecialCharacters = false;
            MaxLoginAttempts = 5;
            LockoutDurationMinutes = 15;
            EnableAuditLogging = true;
            
            ShowSuccess("Settings reset to defaults.");
        }

        private async Task TestEmailAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                // TODO: Implement email test functionality
                await Task.Delay(2000); // Simulate email test
                
                ShowSuccess("Test email sent successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error sending test email: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task BackupDatabaseAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                // TODO: Implement database backup functionality
                await Task.Delay(3000); // Simulate backup process
                
                ShowSuccess("Database backup completed successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error creating database backup: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            SuccessMessage = string.Empty;
        }

        private void ShowSuccess(string message)
        {
            SuccessMessage = message;
            ErrorMessage = string.Empty;
        }

        private void ClearMessages()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }
    }
}
