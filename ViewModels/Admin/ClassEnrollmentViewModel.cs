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
    public partial class ClassEnrollmentViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly Guid _classId;
        
        [ObservableProperty]
        private string _className = string.Empty;
        
        [ObservableProperty]
        private string _teacherName = string.Empty;
        
        [ObservableProperty]
        private bool _isLoading = false;
        
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        [ObservableProperty]
        private string _successMessage = string.Empty;
        
        [ObservableProperty]
        private string _searchText = string.Empty;

        public ObservableCollection<EnrolledStudentViewModel> EnrolledStudents { get; } = new();
        public ObservableCollection<AvailableStudentViewModel> AvailableStudents { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand SearchAvailableStudentsCommand { get; }
        public ICommand AddStudentCommand { get; }
        public ICommand RemoveStudentCommand { get; }
        public ICommand BulkAddCommand { get; }

        public ClassEnrollmentViewModel(Guid classId)
        {
            _classId = classId;
            // Fixed ServiceProvider reference
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
            SearchAvailableStudentsCommand = new AsyncRelayCommand(LoadAvailableStudentsAsync);
            AddStudentCommand = new AsyncRelayCommand<Guid>(AddStudentAsync);
            RemoveStudentCommand = new AsyncRelayCommand<Guid>(RemoveStudentAsync);
            
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await LoadClassInfoAsync();
            await LoadEnrolledStudentsAsync();
            await LoadAvailableStudentsAsync();
        }

        private async Task LoadClassInfoAsync()
        {
            try
            {
                var classEntity = await _dataService.GetClassByIdAsync(_classId);
                if (classEntity != null)
                {
                    ClassName = classEntity.ClassName;
                    TeacherName = classEntity.Teacher?.FullName ?? classEntity.Teacher?.Email ?? "No Teacher";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading class info: {ex.Message}");
            }
        }

        private async Task LoadEnrolledStudentsAsync()
        {
            try
            {
                IsLoading = true;
                var enrollments = await _dataService.GetClassEnrollmentsAsync(_classId);
                
                EnrolledStudents.Clear();
                foreach (var enrollment in enrollments)
                {
                    if (enrollment.Student != null)
                    {
                        var studentViewModel = new EnrolledStudentViewModel
                        {
                            StudentId = enrollment.Student.Id,
                            StudentName = enrollment.Student.FullName ?? enrollment.Student.Email,
                            Email = enrollment.Student.Email,
                            EnrollmentDate = enrollment.EnrollmentDate,
                            RemoveCommand = new AsyncRelayCommand<Guid>(RemoveStudentAsync)
                        };
                        EnrolledStudents.Add(studentViewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading enrolled students: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadAvailableStudentsAsync()
        {
            try
            {
                var availableStudents = await _dataService.GetAvailableStudentsForClassAsync(_classId);
                
                AvailableStudents.Clear();
                foreach (var student in availableStudents)
                {
                    if (string.IsNullOrWhiteSpace(SearchText) || 
                        student.FullName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                        student.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    {
                        var studentViewModel = new AvailableStudentViewModel
                        {
                            StudentId = student.Id,
                            StudentName = student.FullName ?? student.Email,
                            Email = student.Email,
                            IsSelected = false,
                            AddCommand = new AsyncRelayCommand<Guid>(AddStudentAsync)
                        };
                        AvailableStudents.Add(studentViewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading available students: {ex.Message}");
            }
        }

        private async Task AddStudentAsync(Guid studentId)
        {
            try
            {
                var success = await _dataService.EnrollStudentInClassAsync(_classId, studentId);
                if (success)
                {
                    ShowSuccess("Student enrolled successfully.");
                    await LoadDataAsync();
                }
                else
                {
                    ShowError("Student is already enrolled or enrollment failed.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error enrolling student: {ex.Message}");
            }
        }

        private async Task RemoveStudentAsync(Guid studentId)
        {
            try
            {
                var success = await _dataService.RemoveStudentFromClassAsync(_classId, studentId);
                if (success)
                {
                    ShowSuccess("Student removed successfully.");
                    await LoadDataAsync();
                }
                else
                {
                    ShowError("Failed to remove student.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error removing student: {ex.Message}");
            }
        }

        private async Task BulkAddStudentsAsync()
        {
            try
            {
                var selectedStudents = AvailableStudents
                    .Where(s => s.IsSelected)
                    .Select(s => s.StudentId)
                    .ToList();

                if (!selectedStudents.Any())
                {
                    ShowError("Please select students to enroll.");
                    return;
                }

                var success = await _dataService.BulkEnrollStudentsAsync(_classId, selectedStudents);
                if (success)
                {
                    ShowSuccess($"{selectedStudents.Count} students enrolled successfully.");
                    await LoadDataAsync();
                }
                else
                {
                    ShowError("Bulk enrollment failed.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error in bulk enrollment: {ex.Message}");
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            _ = LoadAvailableStudentsAsync();
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
    }

    public partial class EnrolledStudentViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Guid _studentId;
        
        [ObservableProperty]
        private string _studentName = string.Empty;
        
        [ObservableProperty]
        private string _email = string.Empty;
        
        [ObservableProperty]
        private DateTimeOffset _enrollmentDate;

        public ICommand RemoveCommand { get; set; } = null!;
    }

    public partial class AvailableStudentViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Guid _studentId;
        
        [ObservableProperty]
        private string _studentName = string.Empty;
        
        [ObservableProperty]
        private string _email = string.Empty;
        
        [ObservableProperty]
        private bool _isSelected;

        public ICommand AddCommand { get; set; } = null!;
    }
}
