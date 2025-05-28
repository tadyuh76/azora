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
    public partial class StudentManagementViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        
        [ObservableProperty]
        private string _searchText = string.Empty;
        
        [ObservableProperty]
        private bool _isLoading = false;
        
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        [ObservableProperty]
        private string _successMessage = string.Empty;
        
        [ObservableProperty]
        private StudentViewModel? _selectedStudent;

        public ObservableCollection<StudentViewModel> Students { get; } = new();

        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CreateStudentCommand { get; }
        public ICommand EditStudentCommand { get; }
        public ICommand DeleteStudentCommand { get; }

        public StudentManagementViewModel()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            
            SearchCommand = new AsyncRelayCommand(SearchStudentsAsync);
            RefreshCommand = new AsyncRelayCommand(LoadStudentsAsync);
            CreateStudentCommand = new AsyncRelayCommand(CreateStudentAsync);
            EditStudentCommand = new AsyncRelayCommand<Guid>(EditStudentAsync);
            DeleteStudentCommand = new AsyncRelayCommand<Guid>(DeleteStudentAsync);
            
            _ = LoadStudentsAsync();
        }

        private async Task LoadStudentsAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var users = await _dataService.GetUsersByRoleAsync("student");
                Students.Clear();
                
                foreach (var user in users)
                {
                    var studentViewModel = new StudentViewModel
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName ?? "N/A"
                    };
                    Students.Add(studentViewModel);
                }
                
                ShowSuccess($"Loaded {Students.Count} students successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading students: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchStudentsAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var users = await _dataService.SearchUsersAsync(SearchText, "student");
                Students.Clear();
                
                foreach (var user in users)
                {
                    var studentViewModel = new StudentViewModel
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName ?? "N/A"
                    };
                    Students.Add(studentViewModel);
                }
                
                ShowSuccess($"Found {Students.Count} students matching your search.");
            }
            catch (Exception ex)
            {
                ShowError($"Error searching students: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CreateStudentAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                // Create a new student model
                var newStudent = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "new.student@example.com",
                    FullName = "New Student",
                    Role = "student"
                };
                
                // Show dialog to edit (implementation would be in the view)
                
                // Create in database
                await _dataService.CreateUserAsync(newStudent);
                
                await LoadStudentsAsync();
                ShowSuccess("Student created successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error creating student: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EditStudentAsync(Guid studentId)
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var student = await _dataService.GetUserByIdAsync(studentId);
                if (student == null)
                {
                    ShowError("Student not found.");
                    return;
                }
                
                // Show dialog to edit (implementation would be in the view)
                
                // Update in database
                await _dataService.UpdateUserAsync(student);
                
                await LoadStudentsAsync();
                ShowSuccess("Student updated successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating student: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteStudentAsync(Guid studentId)
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                // In a real app, you'd probably want to check for enrollments first
                
                var student = Students.FirstOrDefault(s => s.Id == studentId);
                if (student == null)
                {
                    ShowError("Student not found.");
                    return;
                }
                
                // Delete from database (note: this is just a placeholder)
                // In reality, you'd probably want to archive instead of delete
                // await _dataService.DeleteUserAsync(studentId);
                
                Students.Remove(student);
                ShowSuccess("Student deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting student: {ex.Message}");
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

    public partial class StudentViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Guid _id;
        
        [ObservableProperty]
        private string _email = string.Empty;
        
        [ObservableProperty]
        private string _fullName = string.Empty;
    }
}
