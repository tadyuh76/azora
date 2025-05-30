using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AvaloniaAzora.ViewModels
{
    public partial class AssignTestToClassViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private Guid _classId;

        [ObservableProperty]
        private Guid _teacherId;

        [ObservableProperty]
        private string _className = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Test> _availableTests = new();

        [ObservableProperty]
        private Test? _selectedTest;

        [ObservableProperty]
        private DateTimeOffset _startDate = DateTimeOffset.Now;

        [ObservableProperty]
        private DateTimeOffset _dueDate = DateTimeOffset.Now.AddDays(7);

        [ObservableProperty]
        private int _limitAttempts = 1;

        [ObservableProperty]
        private double _passingScore = 70;

        // Design-time constructor
        public AssignTestToClassViewModel()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
        }

        public AssignTestToClassViewModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task LoadDataAsync(Guid classId, Guid teacherId)
        {
            ClassId = classId;
            TeacherId = teacherId;

            // Load class info
            var classEntity = await _dataService.GetClassByIdAsync(classId);
            if (classEntity != null)
            {
                ClassName = classEntity.ClassName;
            }

            // Load available tests created by this teacher
            var tests = await _dataService.GetTestsByCreatorIdAsync(teacherId);
            AvailableTests.Clear();
            foreach (var test in tests)
            {
                AvailableTests.Add(test);
            }
        }

        [RelayCommand]
        private async Task AssignTest()
        {
            if (SelectedTest == null) return;

            var classTest = new ClassTest
            {
                ClassId = ClassId,
                TestId = SelectedTest.Id,
                StartDate = StartDate.ToUniversalTime(),
                DueDate = DueDate.ToUniversalTime(),
                LimitAttempts = LimitAttempts,
                PassingScore = (float)PassingScore
            };

            await _dataService.AssignTestToClassAsync(classTest);
        }

        // Public method for direct access from view
        public async Task ExecuteAssignTest()
        {
            await AssignTestCommand.ExecuteAsync(null);
        }
    }
}