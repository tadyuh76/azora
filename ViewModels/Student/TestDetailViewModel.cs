using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Services;
using AvaloniaAzora.Models;
using AvaloniaAzora.Views.Student;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaAzora.ViewModels.Student
{
    public partial class TestDetailViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private string _testTitle = string.Empty;

        [ObservableProperty]
        private string _testDescription = string.Empty;

        [ObservableProperty]
        private int _questionCount;

        [ObservableProperty]
        private int _timeLimit;

        [ObservableProperty]
        private int _totalPoints;

        [ObservableProperty]
        private bool _canStartTest = true;

        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private string _buttonText = "Start Test";

        [ObservableProperty]
        private bool _hasPreviousAttempts = false;

        public ObservableCollection<QuestionTypeInfo> QuestionTypes { get; } = new();
        public ObservableCollection<AttemptInfo> PreviousAttempts { get; } = new();

        private Guid _classTestId;
        private Guid _userId;

        // Events
        public event EventHandler? GoBackRequested;
        public event EventHandler<TestStartEventArgs>? StartTestRequested;
        public event EventHandler<ViewAttemptResultEventArgs>? ViewAttemptResultRequested;

        public TestDetailViewModel()
        {
            _dataService = (IDataService)AvaloniaAzora.Services.ServiceProvider.Instance.GetService(typeof(IDataService))!;
        }

        public async Task LoadTestDetailsAsync(Guid classTestId, Guid userId)
        {
            try
            {
                IsLoading = true;
                _classTestId = classTestId;
                _userId = userId;

                Console.WriteLine($"🔍 Loading test details for class test: {classTestId}");

                // Load class test details
                var classTest = await _dataService.GetClassTestByIdAsync(classTestId);
                Console.WriteLine($"🔍 ClassTest loaded: {classTest != null}");
                Console.WriteLine($"🔍 Test in ClassTest: {classTest?.Test != null}");

                if (classTest?.Test == null)
                {
                    Console.WriteLine("❌ Class test or test not found - loading demo data");
                    LoadDemoData(); // Load demo data instead of returning
                    return;
                }

                var test = classTest.Test;

                // Basic test information
                TestTitle = test.Title;
                TestDescription = test.Description ?? "Complete this test to demonstrate your understanding of the material.";
                TimeLimit = test.TimeLimit ?? 45;

                // Load questions for this test
                var questions = await _dataService.GetQuestionsByTestIdAsync(test.Id);
                QuestionCount = questions.Count;
                TotalPoints = questions.Sum(q => q.Points ?? 5); // Default 5 points per question                // Load previous attempts first to check limits
                await LoadPreviousAttemptsAsync();

                // Check if user can start test based on attempt limits
                CanStartTest = CheckCanStartTest(classTest);

                // Analyze question types
                AnalyzeQuestionTypes(questions);                // Load previous attempts first to check limits
                await LoadPreviousAttemptsAsync();

                // Analyze question types
                AnalyzeQuestionTypes(questions);

                Console.WriteLine($"✅ Test details loaded: {TestTitle} ({QuestionCount} questions)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading test details: {ex.Message}");

                // Show demo data for development
                LoadDemoData();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AnalyzeQuestionTypes(List<Question> questions)
        {
            QuestionTypes.Clear();

            var typeGroups = questions.GroupBy(q => q.Type?.ToLower() ?? "multiple_choice").ToList();

            foreach (var group in typeGroups)
            {
                // Only add types that actually have questions
                if (group.Count() > 0)
                {
                    var questionType = new QuestionTypeInfo
                    {
                        TypeName = GetDisplayName(group.Key),
                        Count = group.Count(), // Keep count for internal use
                        TypeColor = GetTypeColor(group.Key)
                    };
                    QuestionTypes.Add(questionType);
                }
            }

            // If no questions, show demo types (only multiple choice and short answer)
            if (!QuestionTypes.Any())
            {
                QuestionTypes.Add(new QuestionTypeInfo { TypeName = "Multiple Choice", Count = 0, TypeColor = "#3B82F6" });
                QuestionTypes.Add(new QuestionTypeInfo { TypeName = "Short Answer", Count = 0, TypeColor = "#F59E0B" });
            }
        }

        private string GetDisplayName(string type)
        {
            return type?.ToLower() switch
            {
                "multiple_choice" => "Multiple Choice",
                "short_answer" => "Short Answer",
                _ => "Multiple Choice"
            };
        }

        private string GetTypeColor(string type)
        {
            return type?.ToLower() switch
            {
                "multiple_choice" => "#3B82F6", // Blue
                "short_answer" => "#F59E0B",    // Orange
                _ => "#6B7280"                  // Gray
            };
        }

        private void LoadDemoData()
        {
            TestTitle = "Algebra Fundamentals Quiz";
            TestDescription = "Test your understanding of basic algebraic concepts including linear equations, quadratic functions, and problem-solving techniques.";
            QuestionCount = 8;
            TimeLimit = 45;
            TotalPoints = 53;
            CanStartTest = true;

            QuestionTypes.Clear();
            QuestionTypes.Add(new QuestionTypeInfo { TypeName = "Multiple Choice", Count = 6, TypeColor = "#3B82F6" });
            QuestionTypes.Add(new QuestionTypeInfo { TypeName = "Short Answer", Count = 2, TypeColor = "#F59E0B" });
        }

        [RelayCommand]
        private void GoBack()
        {
            GoBackRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private async Task StartTest()
        {
            if (!CanStartTest) return;

            try
            {
                Console.WriteLine($"🎯 Starting test: {TestTitle}");

                // Add a minimal delay to make this truly async
                await Task.Delay(1);

                // Raise event to start test
                StartTestRequested?.Invoke(this, new TestStartEventArgs
                {
                    ClassTestId = _classTestId,
                    UserId = _userId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error starting test: {ex.Message}");

                // For demo purposes, still proceed to test taking
                StartTestRequested?.Invoke(this, new TestStartEventArgs
                {
                    ClassTestId = _classTestId,
                    UserId = _userId
                });
            }
        }

        [RelayCommand]
        private void ViewAttemptResult(Guid attemptId)
        {
            Console.WriteLine($"📊 Viewing attempt result: {attemptId}");
            ViewAttemptResultRequested?.Invoke(this, new ViewAttemptResultEventArgs
            {
                AttemptId = attemptId,
                UserId = _userId
            });
        }
        private async Task LoadPreviousAttemptsAsync()
        {
            try
            {
                PreviousAttempts.Clear();

                // Get all attempts for this test by this user
                var attempts = await _dataService.GetAttemptsByStudentAndClassTestAsync(_userId, _classTestId);

                if (attempts.Count > 0)
                {
                    HasPreviousAttempts = true;

                    // Check for incomplete attempts (no EndTime)
                    var incompleteAttempt = attempts.FirstOrDefault(a => !a.EndTime.HasValue);

                    if (incompleteAttempt != null)
                    {
                        // There's an incomplete attempt - allow resuming
                        ButtonText = "Resume Test";
                        Console.WriteLine($"🔄 Found incomplete attempt: {incompleteAttempt.Id}");
                    }
                    else
                    {
                        // Calculate best score from completed attempts
                        var bestScore = attempts.Where(a => a.Score.HasValue).Max(a => a.Score) ?? 0;
                        ButtonText = $"Retake Test (Best: {bestScore:F0}%)";
                    }

                    foreach (var attempt in attempts)
                    {
                        var attemptInfo = new AttemptInfo
                        {
                            AttemptId = attempt.Id,
                            AttemptTitle = $"Attempt #{attempts.IndexOf(attempt) + 1}",
                            DateString = attempt.EndTime?.ToLocalTime().ToString("MMM dd, yyyy, hh:mm tt") ?? "In Progress",
                            ScoreText = attempt.Score?.ToString("F0") + "%" ?? "N/A",
                            PointsText = $"{(attempt.Score ?? 0) * TotalPoints / 100:F0}/{TotalPoints} points",
                            TimeSpent = CalculateTimeSpent(attempt),
                            StatusText = attempt.EndTime.HasValue ? "completed" : "in progress",
                            StatusColor = attempt.EndTime.HasValue ? "#10B981" : "#F59E0B",
                            ScoreColor = GetScoreColor(attempt.Score ?? 0)
                        };

                        PreviousAttempts.Add(attemptInfo);
                    }

                    Console.WriteLine($"📊 Loaded {attempts.Count} previous attempts");
                }
                else
                {
                    HasPreviousAttempts = false;
                    ButtonText = "Start Test";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading previous attempts: {ex.Message}");
                HasPreviousAttempts = false;
                ButtonText = "Start Test";
            }
        }

        private string CalculateTimeSpent(Attempt attempt)
        {
            if (attempt.EndTime.HasValue)
            {
                var timeSpent = attempt.EndTime.Value - attempt.StartTime;
                return $"{timeSpent.Minutes}m";
            }
            return "N/A";
        }
        private bool CheckCanStartTest(ClassTest classTest)
        {
            // Check if there are attempt limits
            if (classTest.LimitAttempts.HasValue && classTest.LimitAttempts > 0)
            {
                // Only count completed attempts towards the limit
                var completedAttemptCount = PreviousAttempts.Count(a => a.StatusText == "completed");
                var canStart = completedAttemptCount < classTest.LimitAttempts.Value;

                if (!canStart)
                {
                    ButtonText = $"Limit Reached ({completedAttemptCount}/{classTest.LimitAttempts})";
                    Console.WriteLine($"⚠️ Cannot start test: attempt limit reached ({completedAttemptCount}/{classTest.LimitAttempts})");
                }
                else
                {
                    Console.WriteLine($"✅ Can start test: {completedAttemptCount}/{classTest.LimitAttempts} completed attempts");
                }

                return canStart;
            }

            // No limits set, can always start
            return true;
        }

        private string GetScoreColor(double score)
        {
            if (score >= 90) return "#10B981"; // Green
            if (score >= 80) return "#3B82F6"; // Blue
            if (score >= 70) return "#F59E0B"; // Orange
            return "#EF4444"; // Red
        }
    }

    public partial class QuestionTypeInfo : ObservableObject
    {
        [ObservableProperty]
        private string _typeName = string.Empty;

        [ObservableProperty]
        private int _count;

        [ObservableProperty]
        private string _typeColor = "#6B7280";
    }

    public partial class AttemptInfo : ObservableObject
    {
        [ObservableProperty]
        private string _attemptTitle = string.Empty;

        [ObservableProperty]
        private string _dateString = string.Empty;

        [ObservableProperty]
        private string _scoreText = string.Empty;

        [ObservableProperty]
        private string _pointsText = string.Empty;

        [ObservableProperty]
        private string _timeSpent = string.Empty;

        [ObservableProperty]
        private string _statusText = string.Empty;

        [ObservableProperty]
        private string _statusColor = string.Empty;

        [ObservableProperty]
        private string _scoreColor = string.Empty;

        public Guid AttemptId { get; set; }
    }

    public class ViewAttemptResultEventArgs : EventArgs
    {
        public Guid AttemptId { get; set; }
        public Guid UserId { get; set; }
    }
}