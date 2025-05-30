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
{    public partial class EditTestViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private Guid _testId;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private int _timeLimit;

        [ObservableProperty]
        private ObservableCollection<Question> _questions = new();

        [ObservableProperty]
        private string _newQuestionText = string.Empty;

        [ObservableProperty]
        private string _newQuestionType = "multiple_choice";

        [ObservableProperty]
        private ObservableCollection<string> _possibleAnswers = new();

        [ObservableProperty]
        private string _correctAnswer = string.Empty;        [ObservableProperty]
        private int _questionPoints = 10;

        // Design-time constructor
        public EditTestViewModel()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();

            // Add some default answer options
            PossibleAnswers.Add("");
            PossibleAnswers.Add("");
            PossibleAnswers.Add("");
            PossibleAnswers.Add("");
        }

        public EditTestViewModel(IDataService dataService)
        {
            _dataService = dataService;

            // Add some default answer options
            PossibleAnswers.Add("");
            PossibleAnswers.Add("");
            PossibleAnswers.Add("");
            PossibleAnswers.Add("");
        }

        partial void OnTitleChanged(string value)
        {
            ValidateProperty(value);
        }

        partial void OnDescriptionChanged(string value)
        {
            ValidateProperty(value);
        }

        partial void OnTimeLimitChanged(int value)
        {
            ValidateProperty(value);
        }

        partial void OnNewQuestionTextChanged(string value)
        {
            ValidateProperty(value);
        }

        partial void OnCorrectAnswerChanged(string value)
        {
            ValidateProperty(value);
        }

        partial void OnQuestionPointsChanged(int value)
        {
            ValidateProperty(value);
        }

        protected new void ValidateProperty<T>(T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null) return;

            ClearPropertyErrors(propertyName);

            switch (propertyName)
            {
                case nameof(Title):                    var titleValidation = _validationService?.ValidateString(value?.ToString(), 3, 100, true);
                    if (titleValidation != null && !titleValidation.IsValid)
                    {
                        foreach (var error in titleValidation.Errors)
                        {
                            AddPropertyError(propertyName, error);
                        }
                    }
                    break;

                case nameof(Description):
                    if (!string.IsNullOrEmpty(value?.ToString()))
                    {                        var descriptionValidation = _validationService?.ValidateString(value.ToString(), 0, 1000, false);
                        if (descriptionValidation != null && !descriptionValidation.IsValid)
                        {
                            foreach (var error in descriptionValidation.Errors)
                            {
                                AddPropertyError(propertyName, error);
                            }
                        }
                    }
                    break;                case nameof(TimeLimit):
                    if (double.TryParse(value?.ToString(), out double timeLimit))
                    {
                        var timeLimitValidation = _validationService?.ValidateNumber(timeLimit, 1, 480);
                        if (timeLimitValidation != null && !timeLimitValidation.IsValid)
                        {
                            foreach (var error in timeLimitValidation.Errors)
                            {
                                AddPropertyError(propertyName, error);
                            }
                        }
                    }
                    break;

                case nameof(NewQuestionText):
                    if (!string.IsNullOrEmpty(value?.ToString()))
                    {                        var questionValidation = _validationService?.ValidateString(value.ToString(), 10, 500, false);
                        if (questionValidation != null && !questionValidation.IsValid)
                        {
                            foreach (var error in questionValidation.Errors)
                            {
                                AddPropertyError(propertyName, error);
                            }
                        }
                    }
                    break;

                case nameof(CorrectAnswer):
                    if (!string.IsNullOrEmpty(value?.ToString()) && NewQuestionType == "short_answer")
                    {                        var answerValidation = _validationService?.ValidateString(value.ToString(), 1, 200, false);
                        if (answerValidation != null && !answerValidation.IsValid)
                        {
                            foreach (var error in answerValidation.Errors)
                            {
                                AddPropertyError(propertyName, error);
                            }
                        }
                    }
                    break;                case nameof(QuestionPoints):
                    if (double.TryParse(value?.ToString(), out double points))
                    {
                        var pointsValidation = _validationService?.ValidateNumber(points, 1, 100);
                        if (pointsValidation != null && !pointsValidation.IsValid)
                        {
                            foreach (var error in pointsValidation.Errors)
                            {
                                AddPropertyError(propertyName, error);
                            }
                        }
                    }
                    break;
            }
        }

        public async Task LoadTestDataAsync(Guid testId)
        {
            var test = await _dataService.GetTestByIdAsync(testId);
            if (test == null) return;

            TestId = test.Id;
            Title = test.Title;
            Description = test.Description ?? string.Empty;
            TimeLimit = test.TimeLimit ?? 60;

            // Load questions
            var questions = await _dataService.GetQuestionsByTestIdAsync(testId);
            Questions.Clear();
            foreach (var question in questions)
            {
                Questions.Add(question);
            }
        }

        public async Task LoadClassTestDataAsync(ClassTest classTest)
        {
            if (classTest.Test == null) return;

            await LoadTestDataAsync(classTest.Test.Id);
        }

        [RelayCommand]
        private async Task UpdateTest()
        {
            // Validate form first            await ValidateTestForm();
            if (HasAnyErrors) return;

            var test = new Test
            {
                Id = TestId,
                Title = Title,
                Description = Description,
                TimeLimit = TimeLimit
            };

            await _dataService.UpdateTestAsync(test);
        }

        [RelayCommand]
        private async Task AddQuestion()
        {            // Validate question first
            await ValidateQuestionForm();
            if (HasAnyErrors) return;

            if (string.IsNullOrWhiteSpace(NewQuestionText))
                return;

            var question = new Question
            {
                TestId = TestId,
                Text = NewQuestionText,
                Type = NewQuestionType,
                Points = QuestionPoints,
                Answers = PossibleAnswers.ToArray(),
                CorrectAnswer = CorrectAnswer
            };

            await _dataService.CreateQuestionAsync(question);

            // Refresh questions
            var questions = await _dataService.GetQuestionsByTestIdAsync(TestId);
            Questions.Clear();
            foreach (var q in questions)
            {
                Questions.Add(q);
            }

            // Clear form
            NewQuestionText = string.Empty;
            CorrectAnswer = string.Empty;

            // Reset possible answers
            PossibleAnswers.Clear();
            PossibleAnswers.Add("");
            PossibleAnswers.Add("");
            PossibleAnswers.Add("");
            PossibleAnswers.Add("");
        }

        [RelayCommand]
        private async Task DeleteQuestion(Question question)
        {
            // TODO: Implement question deletion
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ValidateTestForm()
        {
            // Clear test-related errors
            ClearPropertyErrors(nameof(Title));
            ClearPropertyErrors(nameof(Description));
            ClearPropertyErrors(nameof(TimeLimit));

            // Validate test properties
            ValidateProperty(Title, nameof(Title));
            ValidateProperty(Description, nameof(Description));
            ValidateProperty(TimeLimit, nameof(TimeLimit));

            // Additional business logic validation
            if (string.IsNullOrWhiteSpace(Title))
            {
                AddPropertyError(nameof(Title), "Test title is required");
            }

            if (TimeLimit < 1)
            {
                AddPropertyError(nameof(TimeLimit), "Time limit must be at least 1 minute");
            }

            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ValidateQuestionForm()
        {
            // Clear question-related errors
            ClearPropertyErrors(nameof(NewQuestionText));
            ClearPropertyErrors(nameof(CorrectAnswer));
            ClearPropertyErrors(nameof(QuestionPoints));

            // Validate question properties
            ValidateProperty(NewQuestionText, nameof(NewQuestionText));
            ValidateProperty(CorrectAnswer, nameof(CorrectAnswer));
            ValidateProperty(QuestionPoints, nameof(QuestionPoints));

            // Additional business logic validation
            if (string.IsNullOrWhiteSpace(NewQuestionText))
            {
                AddPropertyError(nameof(NewQuestionText), "Question text is required");
            }
            else if (NewQuestionText.Trim().Length < 10)
            {
                AddPropertyError(nameof(NewQuestionText), "Question text must be at least 10 characters long");
            }

            if (NewQuestionType == "multiple_choice")
            {
                var validAnswers = PossibleAnswers.Where(a => !string.IsNullOrWhiteSpace(a)).Count();
                if (validAnswers < 2)
                {
                    AddPropertyError(nameof(PossibleAnswers), "Please provide at least 2 answer options for multiple choice questions");
                }

                if (string.IsNullOrWhiteSpace(CorrectAnswer))
                {
                    AddPropertyError(nameof(CorrectAnswer), "Please select the correct answer for the multiple choice question");
                }
            }
            else if (NewQuestionType == "short_answer")
            {
                if (string.IsNullOrWhiteSpace(CorrectAnswer))
                {
                    AddPropertyError(nameof(CorrectAnswer), "Please provide the correct answer for the short answer question");
                }
            }

            if (QuestionPoints < 1)
            {
                AddPropertyError(nameof(QuestionPoints), "Question points must be at least 1");
            }

            await Task.CompletedTask;
        }

        public bool IsTestFormValid()
        {
            ValidateTestForm().Wait();
            return !GetErrors(nameof(Title)).Any() && 
                   !GetErrors(nameof(Description)).Any() && 
                   !GetErrors(nameof(TimeLimit)).Any();
        }

        public bool IsQuestionFormValid()
        {
            ValidateQuestionForm().Wait();
            return !GetErrors(nameof(NewQuestionText)).Any() && 
                   !GetErrors(nameof(CorrectAnswer)).Any() && 
                   !GetErrors(nameof(QuestionPoints)).Any() &&
                   !GetErrors(nameof(PossibleAnswers)).Any();
        }
    }
}