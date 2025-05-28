using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaAzora.ViewModels
{
    public partial class TeacherClassroomDetailViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private Guid _classId;

        [ObservableProperty]
        private Guid _teacherId;

        [ObservableProperty]
        private string _className = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private User? _teacher;

        [ObservableProperty]
        private int _studentCount;

        [ObservableProperty]
        private ObservableCollection<User> _students = new();

        [ObservableProperty]
        private ObservableCollection<ClassTest> _tests = new();

        // Design-time constructor
        public TeacherClassroomDetailViewModel()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
        }

        public TeacherClassroomDetailViewModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task LoadClassroomDataAsync(Guid classId)
        {
            var classEntity = await _dataService.GetClassByIdAsync(classId);
            if (classEntity == null) return;

            ClassId = classEntity.Id;
            ClassName = classEntity.ClassName;
            Description = classEntity.Description ?? string.Empty;
            Teacher = classEntity.Teacher;
            TeacherId = classEntity.TeacherId ?? Guid.Empty;

            // Load students enrolled in this class
            var enrollments = await _dataService.GetEnrollmentsByClassIdAsync(classId);
            Students.Clear();
            foreach (var enrollment in enrollments.Where(e => e.Student != null))
            {
                Students.Add(enrollment.Student!);
            }
            StudentCount = Students.Count;

            // Load tests assigned to this class
            var classTests = await _dataService.GetClassTestsByClassIdAsync(classId);
            Tests.Clear();
            foreach (var test in classTests)
            {
                Tests.Add(test);
            }
        }

        [RelayCommand]
        private async Task AddTestToClass(ClassTest classTest)
        {
            await _dataService.AssignTestToClassAsync(classTest);
            await LoadClassroomDataAsync(ClassId);
        }

        [RelayCommand]
        private async Task RemoveTest(Guid classTestId)
        {
            try
            {
                // Get class test entity to verify it exists
                var classTest = await _dataService.GetClassTestByIdAsync(classTestId);
                if (classTest == null) return;

                // Use data service to remove the class test
                await _dataService.RemoveClassTestAsync(classTestId);

                // Refresh data to get the latest changes
                await LoadClassroomDataAsync(ClassId);
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error removing test: {ex.Message}");
            }
        }
    }
}