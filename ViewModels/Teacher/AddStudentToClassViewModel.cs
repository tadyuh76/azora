using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaAzora.ViewModels.Teacher
{
    public partial class AddStudentToClassViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private string _className = string.Empty;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        public ObservableCollection<User> AvailableStudents { get; } = new();
        public ObservableCollection<User> SelectedStudents { get; } = new();

        public AddStudentToClassViewModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task LoadDataAsync(Guid classId)
        {
            IsLoading = true;

            try
            {
                // Load class info
                var classEntity = await _dataService.GetClassByIdAsync(classId);
                if (classEntity != null)
                {
                    ClassName = classEntity.ClassName;
                }

                // Load all users with student role
                var allUsers = await _dataService.GetAllUsersAsync();
                var students = allUsers.Where(u => u.Role == "student").ToList();

                // Load already enrolled students
                var enrollments = await _dataService.GetEnrollmentsByClassIdAsync(classId);
                var enrolledStudentIds = enrollments.Select(e => e.StudentId).ToHashSet();

                // Filter out already enrolled students
                var availableStudents = students.Where(s => !enrolledStudentIds.Contains(s.Id)).ToList();

                AvailableStudents.Clear();
                foreach (var student in availableStudents)
                {
                    AvailableStudents.Add(student);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void AddStudent(User student)
        {
            if (AvailableStudents.Contains(student))
            {
                AvailableStudents.Remove(student);
                SelectedStudents.Add(student);
            }
        }

        [RelayCommand]
        private void RemoveStudent(User student)
        {
            if (SelectedStudents.Contains(student))
            {
                SelectedStudents.Remove(student);
                AvailableStudents.Add(student);
            }
        }

        [RelayCommand]
        private async Task EnrollStudents(Guid classId)
        {
            if (SelectedStudents.Count == 0)
                return;

            IsLoading = true;

            try
            {
                foreach (var student in SelectedStudents.ToList())
                {
                    await _dataService.EnrollStudentAsync(classId, student.Id);
                }

                // Clear selections after successful enrollment
                SelectedStudents.Clear();
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSearchTextChanged(string value)
        {
            // Implement search filtering if needed
            // This can be enhanced to filter AvailableStudents based on search text
        }
    }
}