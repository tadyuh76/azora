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
    public partial class ClassroomManagementViewModel : ViewModelBase
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
        private ClassroomViewModel? _selectedClassroom;

        public ObservableCollection<ClassroomViewModel> Classrooms { get; } = new();

        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CreateClassroomCommand { get; }
        public ICommand EditClassroomCommand { get; }
        public ICommand ViewStudentsCommand { get; }
        public ICommand ViewTestsCommand { get; }
        public ICommand DeleteClassroomCommand { get; }

        public ClassroomManagementViewModel()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            
            SearchCommand = new AsyncRelayCommand(SearchClassroomsAsync);
            RefreshCommand = new AsyncRelayCommand(LoadClassroomsAsync);
            CreateClassroomCommand = new AsyncRelayCommand(CreateClassroomAsync);
            EditClassroomCommand = new AsyncRelayCommand<Guid>(EditClassroomAsync);
            ViewStudentsCommand = new AsyncRelayCommand<Guid>(ViewClassroomStudentsAsync);
            ViewTestsCommand = new AsyncRelayCommand<Guid>(ViewClassroomTestsAsync);
            DeleteClassroomCommand = new AsyncRelayCommand<Guid>(DeleteClassroomAsync);
            
            _ = LoadClassroomsAsync();
        }

        private async Task LoadClassroomsAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var classes = await _dataService.GetAllClassesAsync();
                Classrooms.Clear();
                
                foreach (var classroom in classes)
                {
                    var classroomViewModel = new ClassroomViewModel
                    {
                        Id = classroom.Id,
                        Name = classroom.ClassName,
                        Description = classroom.Description ?? "N/A",
                        TeacherName = classroom.Teacher?.FullName ?? classroom.Teacher?.Email ?? "No Teacher Assigned"
                    };
                    
                    // Get enrollment count
                    var enrollments = await _dataService.GetEnrollmentsByClassIdAsync(classroom.Id);
                    classroomViewModel.StudentCount = enrollments.Count;
                    
                    // Get tests count
                    var tests = await _dataService.GetTestsByClassIdAsync(classroom.Id);
                    classroomViewModel.TestCount = tests.Count;
                    
                    Classrooms.Add(classroomViewModel);
                }
                
                ShowSuccess($"Loaded {Classrooms.Count} classrooms successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading classrooms: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchClassroomsAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var classes = await _dataService.SearchClassesAsync(SearchText);
                Classrooms.Clear();
                
                foreach (var classroom in classes)
                {
                    var classroomViewModel = new ClassroomViewModel
                    {
                        Id = classroom.Id,
                        Name = classroom.ClassName,
                        Description = classroom.Description ?? "N/A",
                        TeacherName = classroom.Teacher?.FullName ?? classroom.Teacher?.Email ?? "No Teacher Assigned"
                    };
                    
                    // Get enrollment count
                    var enrollments = await _dataService.GetEnrollmentsByClassIdAsync(classroom.Id);
                    classroomViewModel.StudentCount = enrollments.Count;
                    
                    // Get tests count
                    var tests = await _dataService.GetTestsByClassIdAsync(classroom.Id);
                    classroomViewModel.TestCount = tests.Count;
                    
                    Classrooms.Add(classroomViewModel);
                }
                
                ShowSuccess($"Found {Classrooms.Count} classrooms matching your search.");
            }
            catch (Exception ex)
            {
                ShowError($"Error searching classrooms: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CreateClassroomAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                // You would typically show a dialog to get user input here
                // For simplicity, we're creating a classroom with placeholder data
                
                var teachers = await _dataService.GetUsersByRoleAsync("teacher");
                var teacher = teachers.FirstOrDefault();
                
                var newClassroom = new Class
                {
                    Id = Guid.NewGuid(),
                    ClassName = "New Classroom",
                    Description = "Classroom Description",
                    TeacherId = teacher?.Id
                };
                
                await _dataService.CreateClassAsync(newClassroom);
                
                await LoadClassroomsAsync();
                ShowSuccess("Classroom created successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error creating classroom: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EditClassroomAsync(Guid classroomId)
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var classroom = await _dataService.GetClassByIdAsync(classroomId);
                if (classroom == null)
                {
                    ShowError("Classroom not found.");
                    return;
                }
                
                // Show dialog to edit (implementation would be in the view)
                
                // Update in database
                await _dataService.UpdateClassAsync(classroom);
                
                await LoadClassroomsAsync();
                ShowSuccess("Classroom updated successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating classroom: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ViewClassroomStudentsAsync(Guid classroomId)
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var enrollments = await _dataService.GetEnrollmentsByClassIdAsync(classroomId);
                
                // In a real app, you'd navigate to a new view to show students
                // For now, just show a message
                
                ShowSuccess($"Classroom has {enrollments.Count} students enrolled.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading classroom students: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ViewClassroomTestsAsync(Guid classroomId)
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var tests = await _dataService.GetTestsByClassIdAsync(classroomId);
                
                // In a real app, you'd navigate to a new view to show tests
                // For now, just show a message
                
                ShowSuccess($"Classroom has {tests.Count} tests assigned.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading classroom tests: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteClassroomAsync(Guid classroomId)
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var classroom = Classrooms.FirstOrDefault(c => c.Id == classroomId);
                if (classroom == null)
                {
                    ShowError("Classroom not found.");
                    return;
                }
                
                // Check if classroom has enrollments or tests
                if (classroom.StudentCount > 0 || classroom.TestCount > 0)
                {
                    ShowError("Cannot delete classroom with students or tests.");
                    return;
                }
                
                // Delete from database
                await _dataService.DeleteClassAsync(classroomId);
                
                Classrooms.Remove(classroom);
                ShowSuccess("Classroom deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting classroom: {ex.Message}");
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

    public partial class ClassroomViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Guid _id;
        
        [ObservableProperty]
        private string _name = string.Empty;
        
        [ObservableProperty]
        private string _description = string.Empty;
        
        [ObservableProperty]
        private string _teacherName = string.Empty;
        
        [ObservableProperty]
        private int _studentCount;
        
        [ObservableProperty]
        private int _testCount;
    }
}
