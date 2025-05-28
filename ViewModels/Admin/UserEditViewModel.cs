using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels.Admin
{
    public partial class UserEditViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly IAuthenticationService _authService;
        
        [ObservableProperty]
        private Guid? _userId;
        
        // Remove all validation attributes since ViewModelBase doesn't inherit from ObservableValidator
        [ObservableProperty]
        private string _email = string.Empty;
        
        [ObservableProperty]
        private string _fullName = string.Empty;
        
        [ObservableProperty]
        private string _selectedRole = "student";
        
        [ObservableProperty]
        private string _password = string.Empty;
        
        [ObservableProperty]
        private string _confirmPassword = string.Empty;
        
        [ObservableProperty]
        private bool _isEditMode = false;
        
        [ObservableProperty]
        private bool _isLoading = false;
        
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        [ObservableProperty]
        private string _successMessage = string.Empty;
        
        [ObservableProperty]
        private bool _sendPasswordResetEmail = false;

        public ObservableCollection<string> AvailableRoles { get; } = new()
        {
            "student", "teacher", "admin"
        };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand GeneratePasswordCommand { get; }

        public event EventHandler? UserSaved;
        public event EventHandler? Cancelled;

        public UserEditViewModel()
        {
            // Fixed ServiceProvider reference
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            _authService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IAuthenticationService>();
            
            SaveCommand = new AsyncRelayCommand(SaveUserAsync);
            CancelCommand = new RelayCommand(Cancel);
            GeneratePasswordCommand = new RelayCommand(GeneratePassword);
        }

        public void SetupForCreate()
        {
            UserId = null;
            IsEditMode = false;
            Email = string.Empty;
            FullName = string.Empty;
            SelectedRole = "student";
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            SendPasswordResetEmail = false;
            ClearMessages();
        }

        public async Task SetupForEditAsync(Guid userId)
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var user = await _dataService.GetUserByIdAsync(userId);
                if (user != null)
                {
                    UserId = user.Id;
                    IsEditMode = true;
                    Email = user.Email;
                    FullName = user.FullName ?? string.Empty;
                    SelectedRole = user.Role;
                    Password = string.Empty;
                    ConfirmPassword = string.Empty;
                    SendPasswordResetEmail = false;
                }
                else
                {
                    ShowError("User not found.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading user: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveUserAsync()
        {
            if (!ValidateInput()) return;

            try
            {
                IsLoading = true;
                ClearMessages();

                if (IsEditMode && UserId.HasValue)
                {
                    await UpdateUserAsync();
                }
                else
                {
                    await CreateUserAsync();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error saving user: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CreateUserAsync()
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                ShowError("Password is required for new users.");
                return;
            }

            try
            {
                // Create user in Supabase Auth
                var authSession = await _authService.SignUpAsync(Email, Password, FullName);
                
                if (authSession?.User?.Id == null)
                {
                    ShowError("Failed to create user account.");
                    return;
                }

                // Create user record in our database
                var user = new User
                {
                    Id = Guid.Parse(authSession.User.Id),
                    Email = Email,
                    FullName = string.IsNullOrWhiteSpace(FullName) ? null : FullName,
                    Role = SelectedRole
                };

                await _dataService.CreateUserAsync(user);
                
                ShowSuccess("User created successfully!");
                UserSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to create user: {ex.Message}");
            }
        }

        private async Task UpdateUserAsync()
        {
            if (!UserId.HasValue) return;

            try
            {
                var existingUser = await _dataService.GetUserByIdAsync(UserId.Value);
                if (existingUser == null)
                {
                    ShowError("User not found.");
                    return;
                }

                // Update user properties that exist in your User model
                existingUser.Email = Email;
                existingUser.FullName = string.IsNullOrWhiteSpace(FullName) ? null : FullName;
                existingUser.Role = SelectedRole;

                await _dataService.UpdateUserAsync(existingUser);
                
                // Handle password reset if requested
                if (SendPasswordResetEmail)
                {
                    await _authService.SendPasswordResetAsync(Email);
                }
                
                ShowSuccess("User updated successfully!");
                UserSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to update user: {ex.Message}");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                ShowError("Email is required.");
                return false;
            }

            if (!IsEditMode)
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    ShowError("Password is required for new users.");
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
            }

            return true;
        }

        private void GeneratePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            var password = new char[12];
            
            for (int i = 0; i < password.Length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }
            
            Password = new string(password);
            ConfirmPassword = Password;
        }

        private void Cancel()
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
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
