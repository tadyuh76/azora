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
    public partial class EditTestViewModel : ObservableObject
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
        private string _correctAnswer = string.Empty;

        [ObservableProperty]
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
        {
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
    }
}