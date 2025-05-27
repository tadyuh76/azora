using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Services;
using AvaloniaAzora.Models;
using AvaloniaAzora.Views.Student;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace AvaloniaAzora.ViewModels.Student
{
    public partial class TestTakingViewModel : ViewModelBase, IDisposable
    {
        private readonly IDataService _dataService;
        private readonly Timer _timer;
        private DateTime _testStartTime;
        private TimeSpan _timeLimit;
        private Guid _attemptId;
        private Guid _userId;
        private Guid _classTestId;
        private bool _disposed = false;

        [ObservableProperty]
        private string _testTitle = string.Empty;

        [ObservableProperty]
        private int _currentQuestionIndex = 0;

        [ObservableProperty]
        private int _totalQuestions = 0;

        [ObservableProperty]
        private string _timeRemainingString = "45:00";

        [ObservableProperty]
        private double _progressPercentage = 0;

        [ObservableProperty]
        private bool _canGoPrevious = false;

        [ObservableProperty]
        private string _nextButtonText = "Next →";

        public ObservableCollection<QuestionViewModel> Questions { get; } = new();

        public QuestionViewModel? CurrentQuestion =>
            Questions.Count > CurrentQuestionIndex ? Questions[CurrentQuestionIndex] : null;

        public int CurrentQuestionNumber => CurrentQuestionIndex + 1;

        // Events
        public event EventHandler<TestCompletedEventArgs>? TestCompleted;
        public event EventHandler? TestAborted;

        public TestTakingViewModel()
        {
            _dataService = (IDataService)AvaloniaAzora.Services.ServiceProvider.Instance.GetService(typeof(IDataService))!;

            // Initialize timer
            _timer = new Timer(1000); // Update every second
            _timer.Elapsed += OnTimerElapsed;
        }

        public Task LoadTestAsync(Guid classTestId, Guid userId)
        {
            // For demo purposes, always load the demo test
            _classTestId = classTestId;
            _userId = userId;

            Console.WriteLine($"🔍 Loading demo test for taking: {classTestId}");
            LoadDemoTest();

            return Task.CompletedTask;
        }

        private void LoadQuestions(List<Question> questions, List<UserAnswer> existingAnswers)
        {
            Questions.Clear();

            for (int i = 0; i < questions.Count; i++)
            {
                var question = questions[i];
                var existingAnswer = existingAnswers.FirstOrDefault(a => a.QuestionId == question.Id);

                var questionViewModel = new QuestionViewModel
                {
                    Id = question.Id,
                    QuestionNumber = i + 1,
                    Text = question.Text,
                    Type = question.Type?.ToLower() ?? "multiple_choice",
                    Points = question.Points ?? 5,
                    IsCurrentQuestion = i == 0
                };

                // Set up question type specific properties
                SetupQuestionType(questionViewModel, question, existingAnswer);

                Questions.Add(questionViewModel);
            }

            TotalQuestions = Questions.Count;
            UpdateProgress();
            UpdateNavigationState();
        }

        private void SetupQuestionType(QuestionViewModel questionViewModel, Question question, UserAnswer? existingAnswer)
        {
            switch (questionViewModel.Type)
            {
                case "multiple_choice":
                    questionViewModel.IsMultipleChoice = true;
                    questionViewModel.QuestionType = "Multiple Choice";
                    questionViewModel.TypeColor = "#3B82F6";

                    if (question.Answers != null && question.Answers.Length > 0)
                    {
                        foreach (var answer in question.Answers)
                        {
                            var option = new AnswerOptionViewModel
                            {
                                Text = answer,
                                IsSelected = existingAnswer?.AnswerText == answer
                            };
                            questionViewModel.AnswerOptions.Add(option);
                        }
                    }
                    else
                    {
                        // Demo options
                        var demoOptions = new[] { "Option A", "Option B", "Option C", "Option D" };
                        foreach (var option in demoOptions)
                        {
                            questionViewModel.AnswerOptions.Add(new AnswerOptionViewModel { Text = option });
                        }
                    }
                    break;

                case "true_false":
                    questionViewModel.IsTrueFalse = true;
                    questionViewModel.QuestionType = "True/False";
                    questionViewModel.TypeColor = "#10B981";

                    if (existingAnswer?.AnswerText == "true")
                        questionViewModel.TrueSelected = true;
                    else if (existingAnswer?.AnswerText == "false")
                        questionViewModel.FalseSelected = true;
                    break;

                case "short_answer":
                    questionViewModel.IsShortAnswer = true;
                    questionViewModel.QuestionType = "Short Answer";
                    questionViewModel.TypeColor = "#F59E0B";
                    questionViewModel.ShortAnswerText = existingAnswer?.AnswerText ?? "";
                    break;

                default:
                    questionViewModel.IsMultipleChoice = true;
                    questionViewModel.QuestionType = "Multiple Choice";
                    questionViewModel.TypeColor = "#3B82F6";
                    break;
            }

            UpdateQuestionStatus(questionViewModel);
        }

        private void LoadDemoTest()
        {
            TestTitle = "Algebra Fundamentals Quiz";
            _timeLimit = TimeSpan.FromMinutes(45);
            _testStartTime = DateTime.Now.AddMinutes(-0.3); // Start slightly in the past to show 44:42
            TimeRemainingString = "44:42"; // Set to match the UI mockup
            _attemptId = Guid.NewGuid(); // Demo attempt

            // Create comprehensive demo questions (8 questions total to match UI)
            Questions.Clear();

            // Question 1 - Multiple Choice (ANSWERED - GREEN)
            var question1 = new QuestionViewModel
            {
                Id = Guid.NewGuid(),
                QuestionNumber = 1,
                Text = "What is the solution to the equation 2x + 5 = 13?",
                Type = "multiple_choice",
                Points = 5,
                IsMultipleChoice = true,
                QuestionType = "Multiple Choice",
                TypeColor = "#3B82F6",
                IsAnswered = true,
                StatusColor = "#10B981",
                StatusTextColor = "White"
            };
            question1.AnswerOptions.Add(new AnswerOptionViewModel { Text = "x = 3", IsSelected = false });
            question1.AnswerOptions.Add(new AnswerOptionViewModel { Text = "x = 4", IsSelected = true });
            question1.AnswerOptions.Add(new AnswerOptionViewModel { Text = "x = 5", IsSelected = false });
            question1.AnswerOptions.Add(new AnswerOptionViewModel { Text = "x = 6", IsSelected = false });
            Questions.Add(question1);

            // Question 2 - True/False (CURRENT QUESTION - DARK)
            var question2 = new QuestionViewModel
            {
                Id = Guid.NewGuid(),
                QuestionNumber = 2,
                Text = "The graph of a quadratic function is always a parabola.",
                Type = "true_false",
                Points = 3,
                IsTrueFalse = true,
                QuestionType = "True False",
                TypeColor = "#10B981",
                IsCurrentQuestion = true,
                TrueSelected = true,
                IsAnswered = true,
                StatusColor = "#10B981",
                StatusTextColor = "White"
            };
            Questions.Add(question2);

            // Question 3 - Multiple Choice (UNANSWERED - GRAY)
            var question3 = new QuestionViewModel
            {
                Id = Guid.NewGuid(),
                QuestionNumber = 3,
                Text = "Which of the following represents a linear function?",
                Type = "multiple_choice",
                Points = 5,
                IsMultipleChoice = true,
                QuestionType = "Multiple Choice",
                TypeColor = "#3B82F6",
                IsAnswered = false,
                StatusColor = "#E5E7EB",
                StatusTextColor = "#6B7280"
            };
            question3.AnswerOptions.Add(new AnswerOptionViewModel { Text = "y = x²" });
            question3.AnswerOptions.Add(new AnswerOptionViewModel { Text = "y = 2x + 1" });
            question3.AnswerOptions.Add(new AnswerOptionViewModel { Text = "y = x³" });
            question3.AnswerOptions.Add(new AnswerOptionViewModel { Text = "y = 1/x" });
            Questions.Add(question3);

            // Question 4 - True/False (UNANSWERED)
            var question4 = new QuestionViewModel
            {
                Id = Guid.NewGuid(),
                QuestionNumber = 4,
                Text = "The slope of a horizontal line is zero.",
                Type = "true_false",
                Points = 3,
                IsTrueFalse = true,
                QuestionType = "True False",
                TypeColor = "#10B981",
                IsAnswered = false,
                StatusColor = "#E5E7EB",
                StatusTextColor = "#6B7280"
            };
            Questions.Add(question4);

            // Question 5 - Multiple Choice (UNANSWERED)
            var question5 = new QuestionViewModel
            {
                Id = Guid.NewGuid(),
                QuestionNumber = 5,
                Text = "What is the y-intercept of the line y = 3x - 7?",
                Type = "multiple_choice",
                Points = 5,
                IsMultipleChoice = true,
                QuestionType = "Multiple Choice",
                TypeColor = "#3B82F6",
                IsAnswered = false,
                StatusColor = "#E5E7EB",
                StatusTextColor = "#6B7280"
            };
            question5.AnswerOptions.Add(new AnswerOptionViewModel { Text = "3" });
            question5.AnswerOptions.Add(new AnswerOptionViewModel { Text = "-7" });
            question5.AnswerOptions.Add(new AnswerOptionViewModel { Text = "7" });
            question5.AnswerOptions.Add(new AnswerOptionViewModel { Text = "-3" });
            Questions.Add(question5);

            // Question 6 - True/False (UNANSWERED)
            var question6 = new QuestionViewModel
            {
                Id = Guid.NewGuid(),
                QuestionNumber = 6,
                Text = "Two parallel lines have the same slope.",
                Type = "true_false",
                Points = 3,
                IsTrueFalse = true,
                QuestionType = "True False",
                TypeColor = "#10B981",
                IsAnswered = false,
                StatusColor = "#E5E7EB",
                StatusTextColor = "#6B7280"
            };
            Questions.Add(question6);

            // Question 7 - Multiple Choice (UNANSWERED)
            var question7 = new QuestionViewModel
            {
                Id = Guid.NewGuid(),
                QuestionNumber = 7,
                Text = "What is the vertex of the parabola y = x² - 4x + 3?",
                Type = "multiple_choice",
                Points = 5,
                IsMultipleChoice = true,
                QuestionType = "Multiple Choice",
                TypeColor = "#3B82F6",
                IsAnswered = false,
                StatusColor = "#E5E7EB",
                StatusTextColor = "#6B7280"
            };
            question7.AnswerOptions.Add(new AnswerOptionViewModel { Text = "(2, -1)" });
            question7.AnswerOptions.Add(new AnswerOptionViewModel { Text = "(1, 0)" });
            question7.AnswerOptions.Add(new AnswerOptionViewModel { Text = "(3, 0)" });
            question7.AnswerOptions.Add(new AnswerOptionViewModel { Text = "(0, 3)" });
            Questions.Add(question7);

            // Question 8 - True/False (UNANSWERED)
            var question8 = new QuestionViewModel
            {
                Id = Guid.NewGuid(),
                QuestionNumber = 8,
                Text = "The domain of f(x) = √x is all real numbers.",
                Type = "true_false",
                Points = 3,
                IsTrueFalse = true,
                QuestionType = "True False",
                TypeColor = "#10B981",
                IsAnswered = false,
                StatusColor = "#E5E7EB",
                StatusTextColor = "#6B7280"
            };
            Questions.Add(question8);

            TotalQuestions = Questions.Count;
            CurrentQuestionIndex = 1; // Start on question 2 (index 1) to match the UI
            UpdateProgress();
            UpdateNavigationState();
            StartTimer();
        }

        private void StartTimer()
        {
            _timer.Start();
            UpdateTimeRemaining();
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            UpdateTimeRemaining();

            // Auto-save every 30 seconds
            if (DateTime.Now.Second % 30 == 0)
            {
                _ = AutoSaveCurrentAnswer();
            }
        }

        private void UpdateTimeRemaining()
        {
            var elapsed = DateTime.Now - _testStartTime;
            var remaining = _timeLimit - elapsed;

            if (remaining <= TimeSpan.Zero)
            {
                TimeRemainingString = "00:00";
                _ = SubmitTest(); // Auto-submit when time runs out
                return;
            }

            TimeRemainingString = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }

        [RelayCommand]
        private void SelectAnswer(AnswerOptionViewModel selectedOption)
        {
            if (CurrentQuestion?.AnswerOptions == null) return;

            // Deselect all options
            foreach (var option in CurrentQuestion.AnswerOptions)
            {
                option.IsSelected = false;
            }

            // Select the chosen option
            selectedOption.IsSelected = true;

            UpdateQuestionStatus(CurrentQuestion);
            UpdateProgress();
            _ = AutoSaveCurrentAnswer();
        }

        [RelayCommand]
        private void SelectTrueFalse(string value)
        {
            if (CurrentQuestion == null) return;

            CurrentQuestion.TrueSelected = value == "true";
            CurrentQuestion.FalseSelected = value == "false";

            UpdateQuestionStatus(CurrentQuestion);
            UpdateProgress();
            _ = AutoSaveCurrentAnswer();
        }

        [RelayCommand]
        private void PreviousQuestion()
        {
            if (CurrentQuestionIndex > 0)
            {
                _ = AutoSaveCurrentAnswer();
                NavigateToQuestion(CurrentQuestionIndex - 1);
            }
        }

        [RelayCommand]
        private async Task NextQuestion()
        {
            await AutoSaveCurrentAnswer();

            if (CurrentQuestionIndex < TotalQuestions - 1)
            {
                NavigateToQuestion(CurrentQuestionIndex + 1);
            }
            else
            {
                // Last question - submit test
                await SubmitTest();
            }
        }

        [RelayCommand]
        private void GoToQuestion(QuestionViewModel question)
        {
            var index = Questions.IndexOf(question);
            if (index >= 0)
            {
                _ = AutoSaveCurrentAnswer();
                NavigateToQuestion(index);
            }
        }

        [RelayCommand]
        private void AbortTest()
        {
            _timer.Stop();
            TestAborted?.Invoke(this, EventArgs.Empty);
        }

        private void NavigateToQuestion(int index)
        {
            // Update current question indicators
            if (CurrentQuestion != null)
                CurrentQuestion.IsCurrentQuestion = false;

            CurrentQuestionIndex = index;

            if (CurrentQuestion != null)
                CurrentQuestion.IsCurrentQuestion = true;

            UpdateNavigationState();
            OnPropertyChanged(nameof(CurrentQuestion));
            OnPropertyChanged(nameof(CurrentQuestionNumber));
        }

        private void UpdateNavigationState()
        {
            CanGoPrevious = CurrentQuestionIndex > 0;
            NextButtonText = CurrentQuestionIndex < TotalQuestions - 1 ? "Next →" : "Submit Test";
        }

        private void UpdateProgress()
        {
            var answeredCount = Questions.Count(q => q.IsAnswered);
            ProgressPercentage = TotalQuestions > 0 ? (double)answeredCount / TotalQuestions * 100 : 0;
        }

        private void UpdateQuestionStatus(QuestionViewModel question)
        {
            bool isAnswered = false;

            switch (question.Type)
            {
                case "multiple_choice":
                    isAnswered = question.AnswerOptions.Any(o => o.IsSelected);
                    break;
                case "true_false":
                    isAnswered = question.TrueSelected || question.FalseSelected;
                    break;
                case "short_answer":
                    isAnswered = !string.IsNullOrWhiteSpace(question.ShortAnswerText);
                    break;
            }

            question.IsAnswered = isAnswered;
            question.StatusColor = isAnswered ? "#10B981" : "#E5E7EB"; // Green if answered, gray if not
            question.StatusTextColor = isAnswered ? "White" : "#6B7280";
        }

        private async Task AutoSaveCurrentAnswer()
        {
            if (CurrentQuestion == null) return;

            try
            {
                string? answerText = null;

                switch (CurrentQuestion.Type)
                {
                    case "multiple_choice":
                        var selectedOption = CurrentQuestion.AnswerOptions.FirstOrDefault(o => o.IsSelected);
                        answerText = selectedOption?.Text;
                        break;
                    case "true_false":
                        if (CurrentQuestion.TrueSelected) answerText = "true";
                        else if (CurrentQuestion.FalseSelected) answerText = "false";
                        break;
                    case "short_answer":
                        answerText = CurrentQuestion.ShortAnswerText;
                        break;
                }

                if (answerText != null)
                {
                    var userAnswer = new UserAnswer
                    {
                        Id = Guid.NewGuid(),
                        AttemptId = _attemptId,
                        QuestionId = CurrentQuestion.Id,
                        AnswerText = answerText
                    };

                    await _dataService.SaveUserAnswerAsync(userAnswer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error auto-saving answer: {ex.Message}");
            }
        }

        private async Task SubmitTest()
        {
            try
            {
                _timer.Stop();

                // Save all answers
                foreach (var question in Questions)
                {
                    var originalCurrent = CurrentQuestion;
                    NavigateToQuestion(Questions.IndexOf(question));
                    await AutoSaveCurrentAnswer();
                }

                // Mark attempt as completed
                var attempt = await _dataService.GetAttemptByIdAsync(_attemptId);
                if (attempt != null)
                {
                    attempt.EndTime = DateTimeOffset.Now;
                    // Score calculation would happen here
                    attempt.Score = CalculateScore();
                    await _dataService.UpdateAttemptAsync(attempt);
                }

                Console.WriteLine($"✅ Test submitted successfully");

                // Raise completion event
                TestCompleted?.Invoke(this, new TestCompletedEventArgs
                {
                    AttemptId = _attemptId,
                    UserId = _userId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error submitting test: {ex.Message}");
            }
        }

        private float CalculateScore()
        {
            // Simple demo scoring
            var answeredQuestions = Questions.Count(q => q.IsAnswered);
            var totalPoints = Questions.Sum(q => q.Points);
            var earnedPoints = answeredQuestions * 5; // Demo: 5 points per answered question

            return totalPoints > 0 ? (float)earnedPoints / totalPoints * 100 : 0;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _timer?.Stop();
                _timer?.Dispose();
                _disposed = true;
            }
        }

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(CurrentQuestion))
            {
                // Update question status when short answer text changes
                if (CurrentQuestion?.IsShortAnswer == true)
                {
                    UpdateQuestionStatus(CurrentQuestion);
                    UpdateProgress();
                }
            }
        }
    }

    public partial class QuestionViewModel : ObservableObject
    {
        [ObservableProperty]
        private Guid _id;

        [ObservableProperty]
        private int _questionNumber;

        [ObservableProperty]
        private string _text = string.Empty;

        [ObservableProperty]
        private string _type = "multiple_choice";

        [ObservableProperty]
        private int _points = 5;

        [ObservableProperty]
        private bool _isCurrentQuestion;

        [ObservableProperty]
        private bool _isAnswered;

        [ObservableProperty]
        private string _statusColor = "#E5E7EB";

        [ObservableProperty]
        private string _statusTextColor = "#6B7280";

        // Question type properties
        [ObservableProperty]
        private bool _isMultipleChoice;

        [ObservableProperty]
        private bool _isTrueFalse;

        [ObservableProperty]
        private bool _isShortAnswer;

        [ObservableProperty]
        private string _questionType = "";

        [ObservableProperty]
        private string _typeColor = "#6B7280";

        // Answer properties
        public ObservableCollection<AnswerOptionViewModel> AnswerOptions { get; } = new();

        [ObservableProperty]
        private bool _trueSelected;

        [ObservableProperty]
        private bool _falseSelected;

        [ObservableProperty]
        private string _shortAnswerText = "";
    }

    public partial class AnswerOptionViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _text = string.Empty;

        [ObservableProperty]
        private bool _isSelected;
    }

    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                return "#3B82F6"; // Blue border when selected/true
            }
            return "#D1D5DB"; // Gray border when not selected/false
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}