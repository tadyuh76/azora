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
    public partial class UserManagementViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        
        [ObservableProperty]
        private string _searchText = string.Empty;
        
        [ObservableProperty]
        private string _selectedRoleFilter = "All";
        
        [ObservableProperty]
        private bool _isLoading = false;
        
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        [ObservableProperty]
        private string _successMessage = string.Empty;
        
        [ObservableProperty]
        private UserListItemViewModel? _selectedUser;
        
        [ObservableProperty]
        private int _totalUsers = 0;
        
        [ObservableProperty]
        private int _activeUsers = 0;
        
        [ObservableProperty]
        private int _inactiveUsers = 0;

        public ObservableCollection<UserListItemViewModel> Users { get; } = new();
        public ObservableCollection<string> RoleFilters { get; } = new()
        {
            "All", "student", "teacher", "admin"
        };

        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CreateUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand ChangeUserRoleCommand { get; }
        public ICommand BulkImportCommand { get; }
        public ICommand ExportUsersCommand { get; }

        public event EventHandler<Guid>? CreateUserRequested;
        public event EventHandler<Guid>? EditUserRequested;
        public event EventHandler? BulkImportRequested;

        public UserManagementViewModel()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            
            SearchCommand = new AsyncRelayCommand(SearchUsersAsync);
            RefreshCommand = new AsyncRelayCommand(LoadUsersAsync);
            CreateUserCommand = new RelayCommand(CreateUser);
            EditUserCommand = new RelayCommand<Guid>(EditUser);
            ChangeUserRoleCommand = new AsyncRelayCommand<Guid>(ChangeUserRoleAsync);
            BulkImportCommand = new RelayCommand(BulkImport);
            ExportUsersCommand = new AsyncRelayCommand(ExportUsersAsync);
            
            _ = LoadUsersAsync();
        }

        public async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var users = await _dataService.GetAllUsersAsync();
                Users.Clear();
                
                foreach (var user in users)
                {
                    var userViewModel = new UserListItemViewModel();
                    userViewModel.UpdateFromUser(user);
                    userViewModel.EditRequested += (s, userId) => EditUser(userId);
                    // Remove ToggleActiveRequested since we don't have IsActive
                    userViewModel.ChangeRoleRequested += (s, userId) => _ = ChangeUserRoleAsync(userId);
                    Users.Add(userViewModel);
                }
                
                await UpdateStatisticsAsync();
                ShowSuccess($"Loaded {Users.Count} users successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading users: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchUsersAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                string? roleFilter = SelectedRoleFilter == "All" ? null : SelectedRoleFilter;
                var users = await _dataService.SearchUsersAsync(SearchText, roleFilter);
                
                Users.Clear();
                
                foreach (var user in users)
                {
                    var userViewModel = new UserListItemViewModel();
                    userViewModel.UpdateFromUser(user);
                    userViewModel.EditRequested += (s, userId) => EditUser(userId);
                    // Remove ToggleActiveRequested since we don't have IsActive
                    userViewModel.ChangeRoleRequested += (s, userId) => _ = ChangeUserRoleAsync(userId);
                    Users.Add(userViewModel);
                }
                
                ShowSuccess($"Found {Users.Count} users matching your search.");
            }
            catch (Exception ex)
            {
                ShowError($"Error searching users: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateStatisticsAsync()
        {
            try
            {
                TotalUsers = await _dataService.GetTotalUsersCountAsync();
                ActiveUsers = await _dataService.GetActiveUsersCountAsync();
                InactiveUsers = TotalUsers - ActiveUsers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user statistics: {ex.Message}");
            }
        }

        private void CreateUser()
        {
            CreateUserRequested?.Invoke(this, Guid.Empty);
        }

        private void EditUser(Guid userId)
        {
            EditUserRequested?.Invoke(this, userId);
        }

        private async Task ChangeUserRoleAsync(Guid userId)
        {
            try
            {
                var userViewModel = Users.FirstOrDefault(u => u.UserId == userId);
                if (userViewModel == null) return;

                IsLoading = true;
                ClearMessages();

                string newRole = userViewModel.Role switch
                {
                    "student" => "teacher",
                    "teacher" => "admin",
                    "admin" => "student",
                    _ => "student"
                };

                var success = await _dataService.ChangeUserRoleAsync(userId, newRole);
                if (success)
                {
                    userViewModel.Role = newRole;
                    userViewModel.RoleColor = newRole.ToLower() switch
                    {
                        "admin" => "#8B5CF6",
                        "teacher" => "#3B82F6",
                        "student" => "#10B981",
                        _ => "#6B7280"
                    };
                    ShowSuccess($"User role changed to {newRole} successfully.");
                }
                else
                {
                    ShowError("Failed to change user role.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error changing user role: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void BulkImport()
        {
            BulkImportRequested?.Invoke(this, EventArgs.Empty);
        }

        private async Task ExportUsersAsync()
        {
            try
            {
                IsLoading = true;
                ShowSuccess("Export functionality would be implemented here.");
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                ShowError($"Error exporting users: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            _ = SearchUsersAsync();
        }

        partial void OnSelectedRoleFilterChanged(string value)
        {
            _ = SearchUsersAsync();
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
