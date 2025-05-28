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
        public event EventHandler<ReviewTestEventArgs>? ReviewTestRequested;

        public TestTakingViewModel()
        {
            _dataService = (IDataService)AvaloniaAzora.Services.ServiceProvider.Instance.GetService(typeof(IDataService))!;

            // Initialize timer
            _timer = new Timer(1000); // Update every second
            _timer.Elapsed += OnTimerElapsed;
        }

        public async Task LoadTestAsync(Guid classTestId, Guid userId)
        {
            _classTestId = classTestId;
            _userId = userId;

            Console.WriteLine($"🔍 Loading test for taking: {classTestId}");

            try
            {
                // Get class test details
                var classTest = await _dataService.GetClassTestByIdAsync(classTestId);
                if (classTest == null || classTest.Test == null)
                {
                    Console.WriteLine("⚠️ Class test or test not found - loading demo test");
                    LoadDemoTest();
                    return;
                }

                // Get test information
                var test = classTest.Test;
                TestTitle = test.Title;
                _timeLimit = TimeSpan.FromMinutes(test.TimeLimit ?? 45);
                _testStartTime = DateTime.Now;

                // Create attempt record with UTC time
                try
                {
                    var attempt = new Attempt
                    {
                        Id = Guid.NewGuid(),
                        StudentId = userId,
                        ClassTestId = classTestId,
                        StartTime = DateTimeOffset.UtcNow // Use UTC time
                    };

                    Console.WriteLine($"🔍 Creating attempt with UTC time: {attempt.StartTime}");

                    var createdAttempt = await _dataService.CreateAttemptAsync(attempt);
                    _attemptId = createdAttempt.Id;
                    Console.WriteLine($"✅ Created attempt: {_attemptId}");

                    // Verify the attempt was created by trying to retrieve it
                    var verifyAttempt = await _dataService.GetAttemptByIdAsync(_attemptId);
                    if (verifyAttempt == null)
                    {
                        Console.WriteLine($"⚠️ Could not verify attempt creation, using demo mode");
                        _attemptId = Guid.NewGuid();
                    }
                    else
                    {
                        Console.WriteLine($"✅ Verified attempt exists in database: {_attemptId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Could not create attempt in database: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"⚠️ Inner exception: {ex.InnerException.Message}");
                    }
                    // Use a temporary GUID for demo purposes
                    _attemptId = Guid.NewGuid();
                    Console.WriteLine($"ℹ️ Using demo attempt ID: {_attemptId}");
                }

                // Get questions for the test
                var questions = await _dataService.GetQuestionsByTestIdAsync(test.Id);

                if (questions.Count == 0)
                {
                    Console.WriteLine("⚠️ No questions found in database - loading demo questions");
                    LoadDemoTest();
                    return;
                }

                // Get any existing answers for this attempt
                var existingAnswers = new List<UserAnswer>();
                try
                {
                    existingAnswers = await _dataService.GetAnswersByAttemptIdAsync(_attemptId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Could not load existing answers: {ex.Message}");
                }

                // Load questions
                LoadQuestions(questions.ToList(), existingAnswers);

                // Start the timer
                StartTimer();

                Console.WriteLine($"✅ Test loaded successfully with {Questions.Count} questions");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading test: {ex.Message}");
                Console.WriteLine("🎯 Loading demo test instead...");
                LoadDemoTest();
            }
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
                    IsCurrentQuestion = i == 0 // Set first question as current
                };

                // Set up question type specific properties
                SetupQuestionType(questionViewModel, question, existingAnswer);

                Questions.Add(questionViewModel);
            }

            TotalQuestions = Questions.Count;

            // Ensure first question is properly selected
            CurrentQuestionIndex = 0;
            if (Questions.Count > 0)
            {
                Questions[0].IsCurrentQuestion = true;
                // Explicitly notify that CurrentQuestion changed
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(CurrentQuestionNumber));
            }

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
                    questionViewModel.Difficulty = CapitalizeFirstLetter(question.Difficulty ?? "medium");

                    if (question.Answers != null && question.Answers.Length > 0)
                    {
                        // Add each answer option with index information
                        for (int i = 0; i < question.Answers.Length; i++)
                        {
                            var optionIndex = i + 1; // 1-based index
                            var option = new AnswerOptionViewModel
                            {
                                Text = question.Answers[i],
                                Index = optionIndex,
                                // Check if this is the option that was previously selected
                                IsSelected = existingAnswer?.AnswerText == optionIndex.ToString()
                            };
                            questionViewModel.AnswerOptions.Add(option);
                        }
                    }
                    else
                    {
                        // Fallback options if none defined
                        var demoOptions = new[] { "Option A", "Option B", "Option C", "Option D" };
                        for (int i = 0; i < demoOptions.Length; i++)
                        {
                            questionViewModel.AnswerOptions.Add(new AnswerOptionViewModel
                            {
                                Text = demoOptions[i],
                                Index = i + 1
                            });
                        }
                    }
                    break;

                case "true_false":
                    // Convert true/false to multiple choice with True/False options
                    questionViewModel.IsMultipleChoice = true;
                    questionViewModel.QuestionType = "Multiple Choice";
                    questionViewModel.TypeColor = "#3B82F6";
                    questionViewModel.Difficulty = CapitalizeFirstLetter(question.Difficulty ?? "easy");

                    // Add True and False as multiple choice options
                    questionViewModel.AnswerOptions.Add(new AnswerOptionViewModel
                    {
                        Text = "True",
                        Index = 1,
                        IsSelected = existingAnswer?.AnswerText == "true"
                    });
                    questionViewModel.AnswerOptions.Add(new AnswerOptionViewModel
                    {
                        Text = "False",
                        Index = 2,
                        IsSelected = existingAnswer?.AnswerText == "false"
                    });
                    break;

                case "short_answer":
                    questionViewModel.IsShortAnswer = true;
                    questionViewModel.QuestionType = "Short Answer";
                    questionViewModel.TypeColor = "#F59E0B";
                    questionViewModel.Difficulty = CapitalizeFirstLetter(question.Difficulty ?? "medium");
                    questionViewModel.ShortAnswerText = existingAnswer?.AnswerText ?? "";
                    break;

                default:
                    questionViewModel.IsMultipleChoice = true;
                    questionViewModel.QuestionType = "Multiple Choice";
                    questionViewModel.TypeColor = "#3B82F6";
                    questionViewModel.Difficulty = CapitalizeFirstLetter(question.Difficulty ?? "medium");
                    break;
            }

            UpdateQuestionStatus(questionViewModel);
        }

        // Helper method to capitalize first letter of a string
        private string CapitalizeFirstLetter(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return char.ToUpper(text[0]) + text.Substring(1).ToLower();
        }

        private void LoadDemoTest()
        {
            Console.WriteLine("🎯 Loading demo test with sample questions");

            // Set basic test info
            TestTitle = "Algebra Fundamentals Quiz";
            _timeLimit = TimeSpan.FromMinutes(45);
            _testStartTime = DateTime.Now;
            _attemptId = Guid.NewGuid(); // Demo attempt ID

            // Clear existing questions
            Questions.Clear();

            // Create demo questions
            var demoQuestions = new List<QuestionViewModel>();

            // Question 1 - Multiple Choice
            var q1 = new QuestionViewModel
            {
                Id = Guid.NewGuid(),
                QuestionNumber = 1,
                Text = "What is the solution to the equation 2x + 5 = 13?",
                Type = "multiple_choice",
                Points = 5,
                IsCurrentQuestion = true,
                IsMultipleChoice = true,
                QuestionType = "Multiple Choice",
                TypeColor = "#3B82F6",
                Difficulty = "Medium"
            };
            q1.AnswerOptions.Add(new AnswerOptionViewModel { Text = "x = 3", Index = 1 });
            q1.AnswerOptions.Add(new AnswerOptionViewModel { Text = "x = 4", Index = 2 });
            q1.AnswerOptions.Add(new AnswerOptionViewModel { Text = "x = 5", Index = 3 });
            q1.AnswerOptions.Add(new AnswerOptionViewModel { Text = "x = 6", Index = 4 });
            demoQuestions.Add(q1);

            // Question 2 - True/False (converted to Multiple Choice)
            var q2 = new QuestionViewModel
            {
                Id = Guid.NewGuid(),
                QuestionNumber = 2,
                Text = "The graph of a quadratic function is always a parabola.",
                Type = "true_false",
                Points = 3,
                IsCurrentQuestion = false,
                IsMultipleChoice = true,
                QuestionType = "Multiple Choice",
                TypeColor = "#3B82F6",
                Difficulty = "Easy"
            };
            q2.AnswerOptions.Add(new AnswerOptionViewModel { Text = "True", Index = 1 });
            q2.AnswerOptions.Add(new AnswerOptionViewModel { Text = "False", Index = 2 });
            demoQuestions.Add(q2);

            // Question 3 - Multiple Choice
            var q3 = new QuestionViewModel
            {
                Id = Guid.NewGuid(),
                QuestionNumber = 3,
                Text = "Which of the following represents a linear function?",
                Type = "multiple_choice",
                Points = 5,
                IsCurrentQuestion = false,
                IsMultipleChoice = true,
                QuestionType = "Multiple Choice",
                TypeColor = "#3B82F6",
                Difficulty = "Medium"
            };
            q3.AnswerOptions.Add(new AnswerOptionViewModel { Text = "y = x²", Index = 1 });
            q3.AnswerOptions.Add(new AnswerOptionViewModel { Text = "y = 2x + 1", Index = 2 });
            q3.AnswerOptions.Add(new AnswerOptionViewModel { Text = "y = x³", Index = 3 });
            q3.AnswerOptions.Add(new AnswerOptionViewModel { Text = "y = 1/x", Index = 4 });
            demoQuestions.Add(q3);

            // Question 4 - Short Answer
            var q4 = new QuestionViewModel
            {
                Id = Guid.NewGuid(),
                QuestionNumber = 4,
                Text = "What is the formula for the area of a circle?",
                Type = "short_answer",
                Points = 5,
                IsCurrentQuestion = false,
                IsShortAnswer = true,
                QuestionType = "Short Answer",
                TypeColor = "#F59E0B",
                Difficulty = "Medium"
            };
            demoQuestions.Add(q4);

            // Question 5 - True/False (converted to Multiple Choice)
            var q5 = new QuestionViewModel
            {
                Id = Guid.NewGuid(),
                QuestionNumber = 5,
                Text = "The slope of a horizontal line is zero.",
                Type = "true_false",
                Points = 3,
                IsCurrentQuestion = false,
                IsMultipleChoice = true,
                QuestionType = "Multiple Choice",
                TypeColor = "#3B82F6",
                Difficulty = "Easy"
            };
            q5.AnswerOptions.Add(new AnswerOptionViewModel { Text = "True", Index = 1 });
            q5.AnswerOptions.Add(new AnswerOptionViewModel { Text = "False", Index = 2 });
            demoQuestions.Add(q5);

            // Add all questions to the collection
            foreach (var question in demoQuestions)
            {
                UpdateQuestionStatus(question);
                Questions.Add(question);
            }

            // Set totals and ensure first question is current
            TotalQuestions = Questions.Count;
            CurrentQuestionIndex = 0;
            if (Questions.Count > 0)
            {
                Questions[0].IsCurrentQuestion = true;
                // Explicitly notify that CurrentQuestion changed
                OnPropertyChanged(nameof(CurrentQuestion));
                OnPropertyChanged(nameof(CurrentQuestionNumber));
            }

            UpdateProgress();
            UpdateNavigationState();

            // Start the timer
            StartTimer();

            Console.WriteLine($"✅ Demo test loaded with {Questions.Count} questions");
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
                _ = SubmitTestCommand.ExecuteAsync(null); // Auto-submit when time runs out
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

            Console.WriteLine($"Selected answer option {selectedOption.Index}: {selectedOption.Text}");
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
                // Last question - show review window instead of directly submitting
                ReviewTest();
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

        [RelayCommand]
        private void ReviewTest()
        {
            // Trigger review event to show review window
            ReviewTestRequested?.Invoke(this, new ReviewTestEventArgs
            {
                Questions = Questions.ToList(),
                TestTitle = TestTitle
            });
        }

        [RelayCommand]
        private async Task SubmitTest()
        {
            try
            {
                _timer.Stop();
                Console.WriteLine("⏱️ Timer stopped for test submission");

                // Save all answers one last time
                foreach (var question in Questions)
                {
                    var originalIndex = CurrentQuestionIndex;

                    // Temporarily navigate to each question to save its answer
                    NavigateToQuestion(Questions.IndexOf(question));
                    await AutoSaveCurrentAnswer();

                    // Restore original position
                    NavigateToQuestion(originalIndex);
                }

                try
                {
                    // Get the attempt from the database
                    var attempt = await _dataService.GetAttemptByIdAsync(_attemptId);
                    if (attempt != null)
                    {
                        // Mark attempt as completed
                        attempt.EndTime = DateTimeOffset.UtcNow; // Use UTC time

                        // Calculate score based on correct answers
                        attempt.Score = await CalculateScoreAsync();

                        // Update the attempt
                        await _dataService.UpdateAttemptAsync(attempt);
                        Console.WriteLine($"✅ Test submitted successfully with score: {attempt.Score:F1}%");

                        // Raise completion event
                        TestCompleted?.Invoke(this, new TestCompletedEventArgs
                        {
                            AttemptId = _attemptId,
                            UserId = _userId,
                            Score = attempt.Score ?? 0
                        });
                    }
                    else
                    {
                        Console.WriteLine("❌ Could not find attempt to submit");

                        // Calculate demo score as fallback
                        var demoScore = await CalculateDemoScore();

                        // Raise completion event with demo data
                        TestCompleted?.Invoke(this, new TestCompletedEventArgs
                        {
                            AttemptId = _attemptId,
                            UserId = _userId,
                            Score = demoScore
                        });
                    }
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"⚠️ Could not submit to database: {dbEx.Message}");
                    Console.WriteLine("🎯 Demo mode: Simulating test completion");

                    // Calculate demo score
                    var demoScore = await CalculateDemoScore();

                    // Raise completion event with demo data
                    TestCompleted?.Invoke(this, new TestCompletedEventArgs
                    {
                        AttemptId = _attemptId,
                        UserId = _userId,
                        Score = demoScore
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error submitting test: {ex.Message}");
                TestAborted?.Invoke(this, EventArgs.Empty);
            }
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
                    isAnswered = question.AnswerOptions.Any(o => o.IsSelected);
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
                        // Save the index (as string) of the selected option
                        answerText = selectedOption?.Index.ToString();
                        break;
                    case "true_false":
                        var selectedTrueFalseOption = CurrentQuestion.AnswerOptions.FirstOrDefault(o => o.IsSelected);
                        // Save the selected text
                        answerText = selectedTrueFalseOption?.Text;
                        break;
                    case "short_answer":
                        answerText = CurrentQuestion.ShortAnswerText?.Trim();
                        break;
                }

                if (answerText != null)
                {
                    try
                    {
                        // Check if this answer already exists (prevent duplicates)
                        var existingAnswers = await _dataService.GetAnswersByAttemptIdAsync(_attemptId);
                        var existingAnswer = existingAnswers.FirstOrDefault(a => a.QuestionId == CurrentQuestion.Id);

                        if (existingAnswer != null)
                        {
                            // Update existing answer only if it's different
                            if (existingAnswer.AnswerText != answerText)
                            {
                                existingAnswer.AnswerText = answerText;
                                existingAnswer.AnsweredAt = DateTimeOffset.UtcNow; // Use UTC time
                                await _dataService.UpdateUserAnswerAsync(existingAnswer);
                                Console.WriteLine($"✅ Updated answer for question {CurrentQuestion.QuestionNumber}: '{answerText}'");
                            }
                        }
                        else
                        {
                            // Create new answer
                            var userAnswer = new UserAnswer
                            {
                                Id = Guid.NewGuid(),
                                AttemptId = _attemptId,
                                QuestionId = CurrentQuestion.Id,
                                AnswerText = answerText,
                                AnsweredAt = DateTimeOffset.UtcNow // Use UTC time
                            };

                            await _dataService.SaveUserAnswerAsync(userAnswer);
                            Console.WriteLine($"✅ Saved new answer for question {CurrentQuestion.QuestionNumber}: '{answerText}'");
                        }
                    }
                    catch (Exception dbEx)
                    {
                        Console.WriteLine($"⚠️ Could not save to database: {dbEx.Message}");
                        if (dbEx.InnerException != null)
                        {
                            Console.WriteLine($"⚠️ Inner exception: {dbEx.InnerException.Message}");
                        }
                        Console.WriteLine($"📝 Demo mode: Answer '{answerText}' recorded locally for question {CurrentQuestion.QuestionNumber}");
                    }

                    // Update UI to show answer is saved
                    UpdateQuestionStatus(CurrentQuestion);
                    UpdateProgress();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error auto-saving answer: {ex.Message}");
            }
        }

        private async Task<float> CalculateScoreAsync()
        {
            try
            {
                // Get all questions for this test from the current Questions collection
                var userAnswers = await _dataService.GetAnswersByAttemptIdAsync(_attemptId);

                if (Questions.Count == 0)
                {
                    return 0;
                }

                float totalPoints = 0;
                float earnedPoints = 0;

                // Use the current Questions collection which already has the test data
                foreach (var questionVM in Questions)
                {
                    // Get the user's answer for this question
                    var userAnswer = userAnswers.FirstOrDefault(a => a.QuestionId == questionVM.Id);

                    // Add to total points
                    totalPoints += questionVM.Points;

                    // Check if the answer is correct by getting the actual Question from database
                    var question = await _dataService.GetQuestionByIdAsync(questionVM.Id);
                    if (question != null)
                    {
                        bool isCorrect = IsAnswerCorrect(question, userAnswer);
                        if (isCorrect)
                        {
                            earnedPoints += questionVM.Points;
                        }
                    }
                }

                // Calculate percentage score
                return totalPoints > 0 ? (earnedPoints / totalPoints) * 100 : 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error calculating score: {ex.Message}");
                return 0;
            }
        }

        private bool IsAnswerCorrect(Question question, UserAnswer? userAnswer)
        {
            if (userAnswer == null || string.IsNullOrEmpty(userAnswer.AnswerText))
                return false;

            // Compare based on question type
            switch (question.Type?.ToLower())
            {
                case "multiple_choice":
                    // For multiple choice, correct_answer should be the index (1-based) of the correct option
                    return string.Equals(userAnswer.AnswerText, question.CorrectAnswer?.Trim(),
                        StringComparison.OrdinalIgnoreCase);

                case "true_false":
                    // For true/false, compare the text values ("True"/"False" vs "true"/"false")
                    var userAnswerText = userAnswer.AnswerText.Trim();
                    var correctAnswerText = question.CorrectAnswer?.Trim();

                    // Handle both "True"/"False" (from UI) and "true"/"false" (from database)
                    return string.Equals(userAnswerText, correctAnswerText, StringComparison.OrdinalIgnoreCase) ||
                           (string.Equals(userAnswerText, "True", StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(correctAnswerText, "true", StringComparison.OrdinalIgnoreCase)) ||
                           (string.Equals(userAnswerText, "False", StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(correctAnswerText, "false", StringComparison.OrdinalIgnoreCase));

                case "short_answer":
                    // For short answer, compare trimmed and lowercased text
                    return string.Equals(
                        userAnswer.AnswerText.Trim().ToLowerInvariant(),
                        question.CorrectAnswer?.Trim().ToLowerInvariant(),
                        StringComparison.OrdinalIgnoreCase);

                default:
                    return false;
            }
        }

        private async Task<float> CalculateDemoScore()
        {
            await Task.Delay(1); // Make it async

            // Simple demo scoring: count answered questions
            int answeredQuestions = 0;
            int totalPoints = 0;
            int earnedPoints = 0;

            foreach (var question in Questions)
            {
                totalPoints += question.Points;

                bool isAnswered = false;
                switch (question.Type)
                {
                    case "multiple_choice":
                        isAnswered = question.AnswerOptions.Any(o => o.IsSelected);
                        break;
                    case "true_false":
                        isAnswered = question.AnswerOptions.Any(o => o.IsSelected);
                        break;
                    case "short_answer":
                        isAnswered = !string.IsNullOrWhiteSpace(question.ShortAnswerText);
                        break;
                }

                if (isAnswered)
                {
                    answeredQuestions++;
                    // Give 70-100% of points randomly for demo
                    earnedPoints += (int)(question.Points * (0.7 + (new Random().NextDouble() * 0.3)));
                }
            }

            var score = totalPoints > 0 ? (float)earnedPoints / totalPoints * 100 : 0;
            Console.WriteLine($"🎯 Demo score calculated: {score:F1}% ({earnedPoints}/{totalPoints} points)");
            return score;
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

        [ObservableProperty]
        private string _difficulty = "medium";
    }

    public partial class AnswerOptionViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _text = string.Empty;

        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private int _index;
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

    public class TestCompletedEventArgs : EventArgs
    {
        public Guid AttemptId { get; set; }
        public Guid UserId { get; set; }
        public float Score { get; set; }
    }

    public class ReviewTestEventArgs : EventArgs
    {
        public List<QuestionViewModel> Questions { get; set; } = new();
        public string TestTitle { get; set; } = string.Empty;
    }
}