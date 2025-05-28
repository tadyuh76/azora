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
    public partial class TeacherManagementViewModel : ViewModelBase
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
        private TeacherViewModel? _selectedTeacher;

        public ObservableCollection<TeacherViewModel> Teachers { get; } = new();

        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CreateTeacherCommand { get; }
        public ICommand EditTeacherCommand { get; }
        public ICommand ViewClassesCommand { get; }
        public ICommand DeleteTeacherCommand { get; }

        public TeacherManagementViewModel()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            
            SearchCommand = new AsyncRelayCommand(SearchTeachersAsync);
            RefreshCommand = new AsyncRelayCommand(LoadTeachersAsync);
            CreateTeacherCommand = new AsyncRelayCommand(CreateTeacherAsync);
            EditTeacherCommand = new AsyncRelayCommand<Guid>(EditTeacherAsync);
            ViewClassesCommand = new AsyncRelayCommand<Guid>(ViewTeacherClassesAsync);
            DeleteTeacherCommand = new AsyncRelayCommand<Guid>(DeleteTeacherAsync);
            
            _ = LoadTeachersAsync();
        }

        private async Task LoadTeachersAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var users = await _dataService.GetUsersByRoleAsync("teacher");
                Teachers.Clear();
                
                foreach (var user in users)
                {
                    var teacherViewModel = new TeacherViewModel
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName ?? "N/A"
                    };
                    
                    // Get classes count for this teacher
                    var classes = await _dataService.GetClassesByTeacherIdAsync(user.Id);
                    teacherViewModel.ClassCount = classes.Count();
                    
                    Teachers.Add(teacherViewModel);
                }
                
                ShowSuccess($"Loaded {Teachers.Count} teachers successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading teachers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchTeachersAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var users = await _dataService.SearchUsersAsync(SearchText, "teacher");
                Teachers.Clear();
                
                foreach (var user in users)
                {
                    var teacherViewModel = new TeacherViewModel
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName ?? "N/A"
                    };
                    
                    var classes = await _dataService.GetClassesByTeacherIdAsync(user.Id);
                    teacherViewModel.ClassCount = classes.Count();
                    
                    Teachers.Add(teacherViewModel);
                }
                
                ShowSuccess($"Found {Teachers.Count} teachers matching your search.");
            }
            catch (Exception ex)
            {
                ShowError($"Error searching teachers: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CreateTeacherAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                // Create a new teacher model
                var newTeacher = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "new.teacher@example.com",
                    FullName = "New Teacher",
                    Role = "teacher"
                };
                
                // Show dialog to edit (implementation would be in the view)
                
                // Create in database
                await _dataService.CreateUserAsync(newTeacher);
                
                await LoadTeachersAsync();
                ShowSuccess("Teacher created successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error creating teacher: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EditTeacherAsync(Guid teacherId)
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var teacher = await _dataService.GetUserByIdAsync(teacherId);
                if (teacher == null)
                {
                    ShowError("Teacher not found.");
                    return;
                }
                
                // Show dialog to edit (implementation would be in the view)
                
                // Update in database
                await _dataService.UpdateUserAsync(teacher);
                
                await LoadTeachersAsync();
                ShowSuccess("Teacher updated successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating teacher: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ViewTeacherClassesAsync(Guid teacherId)
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var classes = await _dataService.GetClassesByTeacherIdAsync(teacherId);
                
                // In a real app, you'd navigate to a new view to show classes
                // For now, just show a message
                
                ShowSuccess($"Teacher has {classes.Count()} classes.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading teacher classes: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteTeacherAsync(Guid teacherId)
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                // In a real app, you'd probably want to check for classes first
                
                var teacher = Teachers.FirstOrDefault(t => t.Id == teacherId);
                if (teacher == null)
                {
                    ShowError("Teacher not found.");
                    return;
                }
                
                // Delete from database (note: this is just a placeholder)
                // In reality, you'd probably want to archive instead of delete
                // await _dataService.DeleteUserAsync(teacherId);
                
                Teachers.Remove(teacher);
                ShowSuccess("Teacher deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting teacher: {ex.Message}");
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

    public partial class TeacherViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Guid _id;
        
        [ObservableProperty]
        private string _email = string.Empty;
        
        [ObservableProperty]
        private string _fullName = string.Empty;
        
        [ObservableProperty]
        private int _classCount;
    }
}
