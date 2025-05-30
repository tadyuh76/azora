using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AvaloniaAzora.ViewModels
{    public partial class AssignTestToClassViewModel : ViewModelBase
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
        private double _passingScore = 70;        // Design-time constructor
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
        }        [RelayCommand]
        private async Task AssignTest()
        {            // Validate form before assignment
            await ValidateForm();
            if (HasAnyErrors) return;

            if (SelectedTest == null)
            {
                AddPropertyError(nameof(SelectedTest), "Please select a test to assign");
                return;
            }

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

        [RelayCommand]
        private async Task ValidateForm()
        {
            // Clear all errors first
            ClearAllErrors();

            // Validate all properties
            ValidateProperty(StartDate, nameof(StartDate));
            ValidateProperty(DueDate, nameof(DueDate));
            ValidateProperty(LimitAttempts, nameof(LimitAttempts));
            ValidateProperty(PassingScore, nameof(PassingScore));

            // Additional business logic validation
            if (SelectedTest == null)
            {
                AddPropertyError(nameof(SelectedTest), "Please select a test to assign");
            }

            if (DueDate <= StartDate)
            {
                AddPropertyError(nameof(DueDate), "Due date must be after start date");
            }

            if (StartDate < DateTimeOffset.Now)
            {
                AddPropertyError(nameof(StartDate), "Start date cannot be in the past");
            }

            if (LimitAttempts < 1)
            {
                AddPropertyError(nameof(LimitAttempts), "Students must be allowed at least 1 attempt");
            }

            if (PassingScore < 0 || PassingScore > 100)
            {
                AddPropertyError(nameof(PassingScore), "Passing score must be between 0 and 100");
            }

            await Task.CompletedTask;
        }        public bool IsFormValid()
        {
            ValidateForm().Wait();
            return !HasAnyErrors;
        }

        // Public method for direct access from view
        public async Task ExecuteAssignTest()
        {
            await AssignTestCommand.ExecuteAsync(null);
        }

        partial void OnStartDateChanged(DateTimeOffset value)
        {
            ValidateProperty(value);
        }

        partial void OnDueDateChanged(DateTimeOffset value)
        {
            ValidateProperty(value);
        }

        partial void OnLimitAttemptsChanged(int value)
        {
            ValidateProperty(value);
        }

        partial void OnPassingScoreChanged(double value)
        {
            ValidateProperty(value);
        }

        protected new void ValidateProperty<T>(T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null) return;

            // Clear existing errors for this property
            ClearPropertyErrors(propertyName);

            switch (propertyName)
            {
                case nameof(StartDate):
                    if (value is DateTimeOffset startDate)
                    {                        var dateValidation = _validationService?.ValidateDate(startDate, DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(1));
                        if (dateValidation != null && !dateValidation.IsValid)
                        {
                            foreach (var error in dateValidation.Errors)
                            {
                                AddPropertyError(propertyName, error);
                            }
                        }
                    }
                    break;

                case nameof(DueDate):
                    if (value is DateTimeOffset dueDate)
                    {                        var dateValidation = _validationService?.ValidateDate(dueDate, StartDate, DateTimeOffset.Now.AddYears(1));
                        if (dateValidation != null && !dateValidation.IsValid)
                        {
                            foreach (var error in dateValidation.Errors)
                            {
                                AddPropertyError(propertyName, error);
                            }
                        }
                        else if (dueDate <= StartDate)
                        {
                            AddPropertyError(propertyName, "Due date must be after start date");
                        }
                    }
                    break;

                case nameof(LimitAttempts):
                    if (value is int attempts)
                    {                        var attemptsValidation = _validationService?.ValidateNumber(attempts, 1, 10);
                        if (attemptsValidation != null && !attemptsValidation.IsValid)
                        {
                            foreach (var error in attemptsValidation.Errors)
                            {
                                AddPropertyError(propertyName, error);
                            }
                        }
                    }
                    break;

                case nameof(PassingScore):
                    if (value is double score)
                    {                        var scoreValidation = _validationService?.ValidateNumber(score, 0, 100);
                        if (scoreValidation != null && !scoreValidation.IsValid)
                        {
                            foreach (var error in scoreValidation.Errors)
                            {
                                AddPropertyError(propertyName, error);
                            }
                        }
                    }
                    break;
            }
        }
    }
}