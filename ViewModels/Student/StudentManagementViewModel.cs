using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Services;
using AvaloniaAzora.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace AvaloniaAzora.ViewModels.Admin
{
    public partial class StudentManagementViewModel : ViewModelBase
    {
        private readonly IClassroomService _classroomService;
        private readonly IUserService _userService;
        private readonly Class _targetClassroom;

        [ObservableProperty]
        private string _classroomName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<User> _enrolledStudents = new();

        [ObservableProperty]
        private ObservableCollection<User> _availableStudents = new(); // Students not yet in this class

        [ObservableProperty]
        private User? _selectedEnrolledStudent;

        [ObservableProperty]
        private User? _selectedAvailableStudent;
        
        [ObservableProperty]
        private string _availableStudentsSearchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public event EventHandler? StudentsUpdated;

        public StudentManagementViewModel(
            IClassroomService classroomService, 
            IUserService userService, 
            Class targetClassroom)
        {
            _classroomService = classroomService;
            _userService = userService;
            _targetClassroom = targetClassroom;
            ClassroomName = targetClassroom.ClassName;

            LoadStudentsCommand = new AsyncRelayCommand(LoadStudentsAsync);
            AddStudentCommand = new AsyncRelayCommand(AddStudentAsync, CanAddStudent);
            RemoveStudentCommand = new AsyncRelayCommand(RemoveStudentAsync, CanRemoveStudent);
            CloseDialogCommand = new RelayCommand(CloseDialog);
            SearchAvailableStudentsCommand = new AsyncRelayCommand(SearchAvailableStudentsAsync);

            _ = LoadStudentsAsync();
        }
        
        partial void OnAvailableStudentsSearchTextChanged(string value)
        {
            _ = SearchAvailableStudentsAsync();
        }

        public IAsyncRelayCommand LoadStudentsCommand { get; }
        public IAsyncRelayCommand AddStudentCommand { get; }
        public IAsyncRelayCommand RemoveStudentCommand { get; }
        public IRelayCommand CloseDialogCommand { get; }
        public IAsyncRelayCommand SearchAvailableStudentsCommand { get; }


        private async Task LoadStudentsAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Load enrolled students
                var enrolled = await _classroomService.GetClassroomStudentsAsync(_targetClassroom.ClassId);
                EnrolledStudents.Clear();
                foreach (var student in enrolled.OrderBy(s => s.LastName))
                {
                    EnrolledStudents.Add(student);
                }

                // Load all potential students and filter out those already enrolled
                await SearchAvailableStudentsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading students: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(ErrorMessage);
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task SearchAvailableStudentsAsync()
        {
            try
            {
                IsLoading = true;
                var allStudents = await _userService.GetUsersByRoleAsync("Student");
                var enrolledStudentIds = EnrolledStudents.Select(s => s.UserId).ToHashSet();

                var filteredAvailable = allStudents
                    .Where(s => !enrolledStudentIds.Contains(s.UserId))
                    .Where(s => string.IsNullOrWhiteSpace(AvailableStudentsSearchText) || 
                                s.FullName.ToLower().Contains(AvailableStudentsSearchText.ToLower()) ||
                                s.Email.ToLower().Contains(AvailableStudentsSearchText.ToLower()))
                    .OrderBy(s => s.LastName)
                    .ToList();

                AvailableStudents.Clear();
                foreach (var student in filteredAvailable)
                {
                    AvailableStudents.Add(student);
                }
            }
            catch (Exception ex)
            {
                 ErrorMessage = $"Error searching available students: {ex.Message}";
                 System.Diagnostics.Debug.WriteLine(ErrorMessage);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanAddStudent() => SelectedAvailableStudent != null && !IsLoading;
        private async Task AddStudentAsync()
        {
            if (SelectedAvailableStudent == null) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                bool success = await _classroomService.EnrollStudentAsync(_targetClassroom.ClassId, SelectedAvailableStudent.UserId);
                if (success)
                {
                    await LoadStudentsAsync(); // Refresh both lists
                }
                else
                {
                    ErrorMessage = "Failed to add student. They might already be enrolled.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding student: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                AddStudentCommand.NotifyCanExecuteChanged();
            }
        }

        private bool CanRemoveStudent() => SelectedEnrolledStudent != null && !IsLoading;
        private async Task RemoveStudentAsync()
        {
            if (SelectedEnrolledStudent == null) return;
            
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                bool success = await _classroomService.RemoveStudentAsync(_targetClassroom.ClassId, SelectedEnrolledStudent.UserId);
                if (success)
                {
                    await LoadStudentsAsync(); // Refresh both lists
                }
                else
                {
                    ErrorMessage = "Failed to remove student.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error removing student: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                RemoveStudentCommand.NotifyCanExecuteChanged();
            }
        }
        
        partial void OnSelectedAvailableStudentChanged(User? value) => AddStudentCommand.NotifyCanExecuteChanged();
        partial void OnSelectedEnrolledStudentChanged(User? value) => RemoveStudentCommand.NotifyCanExecuteChanged();


        private void CloseDialog()
        {
            StudentsUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}
