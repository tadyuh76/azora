using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Services;
using AvaloniaAzora.Models;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaAzora.ViewModels.Student
{
    public partial class TestResultViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly GroqApiService _groqApiService;

        [ObservableProperty]
        private string _testTitle = string.Empty;

        [ObservableProperty]
        private double _scorePercentage = 0;

        [ObservableProperty]
        private int _correctAnswers = 0;

        [ObservableProperty]
        private int _incorrectAnswers = 0;

        [ObservableProperty]
        private int _pointsEarned = 0;

        [ObservableProperty]
        private int _totalPoints = 0;

        [ObservableProperty]
        private int _totalQuestions = 0;

        [ObservableProperty]
        private string _classRank = "#8";

        [ObservableProperty]
        private string _totalStudents = "32";

        [ObservableProperty]
        private double _classAverageScore = 76.0;

        [ObservableProperty]
        private double _scoreDifference = 5.0;

        [ObservableProperty]
        private string _completedDateString = string.Empty;

        [ObservableProperty]
        private string _timeTakenString = string.Empty;

        [ObservableProperty]
        private string _scoreColor = "#10B981";

        [ObservableProperty]
        private string _scoreBackgroundColor = "#10B981";

        [ObservableProperty]
        private string _performanceBadgeText = "Good";

        [ObservableProperty]
        private string _performanceBadgeColor = "#10B981";

        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private bool _isLoadingInsights = false;

        [ObservableProperty]
        private string _aiStrengths = string.Empty;

        [ObservableProperty]
        private string _aiAreasToImprove = string.Empty;

        [ObservableProperty]
        private string _aiRecommendations = string.Empty;

        [ObservableProperty]
        private bool _hasAiInsights = false;

        [ObservableProperty]
        private bool _hasInsightsError = false;

        [ObservableProperty]
        private string _aiExplanation = string.Empty;

        [ObservableProperty]
        private bool _hasExplanationError = false;

        [ObservableProperty]
        private string _difficulty = "medium";

        [ObservableProperty]
        private double _classCorrectPercentage = 0;

        [ObservableProperty]
        private string _classPerformanceText = "";

        [ObservableProperty]
        private string _classPerformanceColor = "#6B7280";

        [ObservableProperty]
        private bool _hasMultipleAttempts = false;

        [ObservableProperty]
        private int _currentAttemptNumber = 1;

        [ObservableProperty]
        private int _totalAttempts = 1;

        [ObservableProperty]
        private string _attemptInfo = "Attempt 1 of 1";

        [ObservableProperty]
        private bool _canGoPrevious = false;

        [ObservableProperty]
        private bool _canGoNext = false;

        [ObservableProperty]
        private string _categoryName = "General";

        public ObservableCollection<QuestionResultViewModel> QuestionResults { get; } = new();
        public List<Attempt> AllAttempts { get; private set; } = new();
        public Guid CurrentAttemptId { get; private set; }
        public Guid CurrentUserId { get; private set; }
        public Guid CurrentClassTestId { get; private set; }

        // Events
        public event EventHandler? GoBackRequested;

        public TestResultViewModel()
        {
            _dataService = (IDataService)AvaloniaAzora.Services.ServiceProvider.Instance.GetService(typeof(IDataService))!;
            _groqApiService = new GroqApiService();
        }

        public async Task LoadResultsAsync(Guid attemptId, Guid userId)
        {
            try
            {
                IsLoading = true;
                CurrentAttemptId = attemptId;
                CurrentUserId = userId;

                Console.WriteLine($"🔍 Loading test results for attempt: {attemptId}");

                // Load attempt details
                var attempt = await _dataService.GetAttemptByIdAsync(attemptId);
                if (attempt?.ClassTest?.Test == null)
                {
                    Console.WriteLine("❌ Attempt, class test, or test not found");
                    LoadDemoResults();
                    return;
                }

                CurrentClassTestId = attempt.ClassTestId ?? Guid.Empty;
                var test = attempt.ClassTest.Test;
                TestTitle = test.Title;

                // Load all attempts for this test by this student
                if (attempt.ClassTestId.HasValue)
                {
                    AllAttempts = await _dataService.GetAttemptsByStudentAndClassTestAsync(userId, attempt.ClassTestId.Value);
                    TotalAttempts = AllAttempts.Count;
                    CurrentAttemptNumber = AllAttempts.FindIndex(a => a.Id == attemptId) + 1;
                    HasMultipleAttempts = TotalAttempts > 1;
                    AttemptInfo = $"Attempt {CurrentAttemptNumber} of {TotalAttempts}";

                    UpdateNavigationStates();
                    Console.WriteLine($"📊 Found {TotalAttempts} attempts for this test");
                }

                // Calculate completion info
                if (attempt.EndTime.HasValue)
                {
                    CompletedDateString = attempt.EndTime.Value.ToString("MMMM dd, yyyy 'at' hh:mm tt");
                    var timeTaken = attempt.EndTime.Value - attempt.StartTime;
                    TimeTakenString = $"{timeTaken.Minutes}m {timeTaken.Seconds}s";
                }

                // Load questions and user answers
                var questions = await _dataService.GetQuestionsByTestIdAsync(test.Id);
                var userAnswers = await _dataService.GetAnswersByAttemptIdAsync(attemptId);

                if (questions.Count == 0)
                {
                    Console.WriteLine("⚠️ No questions found, loading demo");
                    LoadDemoResults();
                    return;
                }

                // Calculate scores and create question results
                CalculateScores(questions, userAnswers);

                // Calculate class ranking
                if (attempt.ClassTestId.HasValue)
                {
                    await CalculateClassRankingAsync(attempt.ClassTestId.Value, attemptId, ScorePercentage);
                }

                // Load AI insights in background
                _ = Task.Run(async () => await LoadAIInsightsAsync());

                // Load class performance data in background
                if (attempt.ClassTestId.HasValue)
                {
                    _ = Task.Run(async () => await LoadClassPerformanceAsync(attempt.ClassTestId.Value));
                }

                Console.WriteLine($"✅ Test results loaded: {TestTitle} ({ScorePercentage:F1}%)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading test results: {ex.Message}");
                LoadDemoResults();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CalculateScores(List<Question> questions, List<UserAnswer> userAnswers)
        {
            QuestionResults.Clear();

            int correctCount = 0;
            int totalPointsValue = 0;
            int earnedPointsValue = 0;

            for (int i = 0; i < questions.Count; i++)
            {
                var question = questions[i];
                var userAnswer = userAnswers.FirstOrDefault(a => a.QuestionId == question.Id);

                var questionPoints = question.Points ?? 5;
                totalPointsValue += questionPoints;

                // Determine if answer is correct
                bool isCorrect = IsAnswerCorrect(question, userAnswer);
                if (isCorrect)
                {
                    correctCount++;
                    earnedPointsValue += questionPoints;
                }

                var questionResult = new QuestionResultViewModel
                {
                    QuestionNumber = i + 1,
                    QuestionText = question.Text,
                    QuestionType = GetDisplayQuestionType(question.Type),
                    TypeColor = GetTypeColor(question.Type),
                    UserAnswer = FormatUserAnswerForDisplay(question, userAnswer),
                    CorrectAnswer = FormatCorrectAnswerForDisplay(question),
                    IsCorrect = isCorrect,
                    TotalPoints = questionPoints,
                    PointsEarned = isCorrect ? questionPoints : 0,
                    StatusIcon = isCorrect ? "✅" : "❌",
                    Difficulty = CapitalizeFirstLetter(question.Difficulty ?? "medium"),
                    Explanation = GetQuestionExplanation(question, isCorrect),
                    HasExplanation = true,
                    QuestionTextValue = question.Text,
                    AnswerOptions = question.Answers ?? Array.Empty<string>(),
                    CorrectAnswerValue = question.CorrectAnswer ?? "",
                    QuestionId = question.Id,
                    CategoryName = question.Category?.Name ?? "General"
                };

                QuestionResults.Add(questionResult);
            }

            // Update summary
            CorrectAnswers = correctCount;
            IncorrectAnswers = questions.Count - correctCount;
            TotalQuestions = questions.Count;
            TotalPoints = totalPointsValue;
            PointsEarned = earnedPointsValue;
            ScorePercentage = totalPointsValue > 0 ? (double)earnedPointsValue / totalPointsValue * 100 : 0;

            // Update UI colors and badges based on score
            UpdateScoreDisplay();
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

        private string FormatUserAnswerForDisplay(Question question, UserAnswer? userAnswer)
        {
            if (userAnswer == null || string.IsNullOrEmpty(userAnswer.AnswerText))
                return "No answer";

            switch (question.Type?.ToLower())
            {
                case "multiple_choice":
                    // Convert index to actual option text
                    if (int.TryParse(userAnswer.AnswerText, out int index) &&
                        question.Answers != null &&
                        index > 0 && index <= question.Answers.Length)
                    {
                        return question.Answers[index - 1];
                    }
                    return $"Option {userAnswer.AnswerText}";

                case "short_answer":
                    // Return as is
                    return userAnswer.AnswerText;

                default:
                    return userAnswer.AnswerText;
            }
        }

        private string FormatCorrectAnswerForDisplay(Question question)
        {
            if (string.IsNullOrEmpty(question.CorrectAnswer))
                return "Not specified";

            switch (question.Type?.ToLower())
            {
                case "multiple_choice":
                    // Convert index to actual option text
                    if (int.TryParse(question.CorrectAnswer, out int index) &&
                        question.Answers != null &&
                        index > 0 && index <= question.Answers.Length)
                    {
                        return question.Answers[index - 1];
                    }
                    return $"Option {question.CorrectAnswer}";

                case "short_answer":
                    // Return as is
                    return question.CorrectAnswer;

                default:
                    return question.CorrectAnswer;
            }
        }

        private string GetQuestionExplanation(Question question, bool isCorrect)
        {
            if (isCorrect) return "";

            // Provide demo explanations
            switch (question.Type?.ToLower())
            {
                case "multiple_choice":
                    return "Subtract 5 from both sides: 2x = 8, then divide by 2: x = 4";
                case "short_answer":
                    return "A linear equation has degree 1 (e.g., y = 2x + 3), while a quadratic equation has degree 2 (e.g., y = x² + 2x + 1).";
                default:
                    return "";
            }
        }

        private string GetDisplayQuestionType(string? type)
        {
            return type?.ToLower() switch
            {
                "multiple_choice" => "Multiple Choice",
                "short_answer" => "Short Answer",
                _ => "Multiple Choice"
            };
        }

        private string GetTypeColor(string? type)
        {
            return type?.ToLower() switch
            {
                "multiple_choice" => "#3B82F6",
                "short_answer" => "#F59E0B",
                _ => "#6B7280"
            };
        }

        private void UpdateScoreDisplay()
        {
            if (ScorePercentage >= 90)
            {
                ScoreColor = "#10B981"; // Green
                ScoreBackgroundColor = "#10B981";
                PerformanceBadgeColor = "#10B981";
                PerformanceBadgeText = "Excellent";
            }
            else if (ScorePercentage >= 80)
            {
                ScoreColor = "#3B82F6"; // Blue
                ScoreBackgroundColor = "#3B82F6";
                PerformanceBadgeColor = "#3B82F6";
                PerformanceBadgeText = "Good";
            }
            else if (ScorePercentage >= 70)
            {
                ScoreColor = "#F59E0B"; // Orange
                ScoreBackgroundColor = "#F59E0B";
                PerformanceBadgeColor = "#F59E0B";
                PerformanceBadgeText = "Average";
            }
            else
            {
                ScoreColor = "#EF4444"; // Red
                ScoreBackgroundColor = "#EF4444";
                PerformanceBadgeColor = "#EF4444";
                PerformanceBadgeText = "Needs Improvement";
            }
        }

        private void LoadDemoResults()
        {
            TestTitle = "Algebra Fundamentals Quiz";
            ScorePercentage = 81;
            CorrectAnswers = 3;
            IncorrectAnswers = 2;
            TotalQuestions = 5;
            PointsEarned = 22;
            TotalPoints = 27;
            ClassRank = "#8";
            CompletedDateString = "January 8, 2024 at 09:38 PM";
            TimeTakenString = "38m 15s";

            QuestionResults.Clear();

            // Question 1 - Correct Multiple Choice
            QuestionResults.Add(new QuestionResultViewModel
            {
                QuestionNumber = 1,
                QuestionText = "What is the solution to the equation 2x + 5 = 13?",
                QuestionType = "Multiple Choice",
                TypeColor = "#3B82F6",
                UserAnswer = "x = 4",
                CorrectAnswer = "x = 4",
                IsCorrect = true,
                TotalPoints = 5,
                PointsEarned = 5,
                StatusIcon = "✅",
                CategoryName = "Algebra"
            });

            // Question 2 - Incorrect Multiple Choice
            QuestionResults.Add(new QuestionResultViewModel
            {
                QuestionNumber = 2,
                QuestionText = "Which of the following is equivalent to 3(x + 2)?",
                QuestionType = "Multiple Choice",
                TypeColor = "#3B82F6",
                UserAnswer = "3x + 2",
                CorrectAnswer = "3x + 6",
                IsCorrect = false,
                TotalPoints = 5,
                PointsEarned = 0,
                StatusIcon = "❌",
                Explanation = "When distributing 3 to (x + 2), multiply both terms: 3×x + 3×2 = 3x + 6",
                HasExplanation = true,
                CategoryName = "Algebra"
            });

            // Question 3 - Correct Short Answer
            QuestionResults.Add(new QuestionResultViewModel
            {
                QuestionNumber = 3,
                QuestionText = "Explain the difference between a linear and quadratic equation. Provide examples of each.",
                QuestionType = "Short Answer",
                TypeColor = "#F59E0B",
                UserAnswer = "A linear equation has degree 1 (e.g., y = 2x + 3), while a quadratic equation has degree 2 (e.g., y = x² + 2x + 1). Linear equations graph as straight lines, quadratic equations graph as parabolas.",
                CorrectAnswer = "Sample answer provided",
                IsCorrect = true,
                TotalPoints = 10,
                PointsEarned = 10,
                StatusIcon = "✅",
                CategoryName = "Functions"
            });

            // Question 4 - Incorrect Multiple Choice
            QuestionResults.Add(new QuestionResultViewModel
            {
                QuestionNumber = 4,
                QuestionText = "What is the slope of the line y = -2x + 5?",
                QuestionType = "Multiple Choice",
                TypeColor = "#3B82F6",
                UserAnswer = "5",
                CorrectAnswer = "-2",
                IsCorrect = false,
                TotalPoints = 4,
                PointsEarned = 0,
                StatusIcon = "❌",
                CategoryName = "Linear Functions"
            });

            // Question 5 - Correct Short Answer
            QuestionResults.Add(new QuestionResultViewModel
            {
                QuestionNumber = 5,
                QuestionText = "Solve for x: 2x - 3 = 7",
                QuestionType = "Short Answer",
                TypeColor = "#F59E0B",
                UserAnswer = "5",
                CorrectAnswer = "5",
                IsCorrect = true,
                TotalPoints = 3,
                PointsEarned = 3,
                StatusIcon = "✅",
                CategoryName = "Algebra"
            });

            UpdateScoreDisplay();
        }

        [RelayCommand]
        private void GoBack()
        {
            GoBackRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void ViewAttempts()
        {
            // TODO: Implement view attempts functionality
            Console.WriteLine("📊 View attempts requested");
        }

        [RelayCommand]
        public async Task RetryInsights()
        {
            HasInsightsError = false;
            await LoadAIInsightsAsync();
        }

        [RelayCommand]
        public async Task PreviousAttempt()
        {
            if (CurrentAttemptNumber > 1)
            {
                var previousAttempt = AllAttempts[CurrentAttemptNumber - 2];
                await LoadResultsAsync(previousAttempt.Id, CurrentUserId);
            }
        }

        [RelayCommand]
        public async Task NextAttempt()
        {
            if (CurrentAttemptNumber < TotalAttempts)
            {
                var nextAttempt = AllAttempts[CurrentAttemptNumber];
                await LoadResultsAsync(nextAttempt.Id, CurrentUserId);
            }
        }

        [RelayCommand]
        private void ViewAllAttempts()
        {
            // TODO: Open a dialog or window to show all attempts
            Console.WriteLine("📊 View all attempts requested");
            foreach (var attempt in AllAttempts)
            {
                var score = attempt.Score?.ToString("F1") ?? "N/A";
                var date = attempt.EndTime?.ToString("MMM dd, yyyy") ?? "In Progress";
                Console.WriteLine($"  - Attempt {AllAttempts.IndexOf(attempt) + 1}: {score}% on {date}");
            }
        }

        // Helper method to capitalize first letter of a string
        private string CapitalizeFirstLetter(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return char.ToUpper(text[0]) + text.Substring(1).ToLower();
        }

        private async Task CalculateClassRankingAsync(Guid classTestId, Guid currentAttemptId, double currentScore)
        {
            try
            {
                // Get all attempts for this class test
                var allAttempts = await _dataService.GetAttemptsByClassTestIdAsync(classTestId);

                if (allAttempts?.Count > 0)
                {
                    // Filter out attempts without scores and get unique students
                    var validAttempts = allAttempts
                        .Where(a => a.Score.HasValue)
                        .GroupBy(a => a.StudentId)
                        .Select(g => g.OrderByDescending(a => a.Score).First()) // Take best attempt per student
                        .ToList();

                    if (validAttempts.Count > 0)
                    {
                        // Calculate class average
                        ClassAverageScore = validAttempts.Average(a => a.Score ?? 0);

                        // Calculate rank (higher scores get better ranks)
                        var rankedAttempts = validAttempts
                            .OrderByDescending(a => a.Score)
                            .ToList();

                        var rankIndex = rankedAttempts.FindIndex(a => a.Id == currentAttemptId);

                        // If attempt not found in ranked list, calculate rank by score
                        int rank;
                        if (rankIndex >= 0)
                        {
                            rank = rankIndex + 1;
                        }
                        else
                        {
                            // Count how many attempts have better scores
                            rank = rankedAttempts.Count(a => a.Score > currentScore) + 1;
                        }

                        ClassRank = $"#{rank}";
                        TotalStudents = validAttempts.Count.ToString();
                        ScoreDifference = currentScore - ClassAverageScore;

                        Console.WriteLine($"📊 Ranking: {rank}/{validAttempts.Count}, Class avg: {ClassAverageScore:F1}%");
                        return;
                    }
                }

                // Fallback values
                ClassRank = "#1";
                TotalStudents = "1";
                ClassAverageScore = currentScore;
                ScoreDifference = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error calculating ranking: {ex.Message}");
                // Set default values
                ClassRank = "#1";
                TotalStudents = "1";
                ClassAverageScore = currentScore;
                ScoreDifference = 0;
            }
        }

        private async Task LoadAIInsightsAsync()
        {
            try
            {
                IsLoadingInsights = true;
                HasInsightsError = false;

                // Calculate category performance
                var categoryPerformance = new Dictionary<string, CategoryPerformance>();
                foreach (var questionResult in QuestionResults)
                {
                    // Get category from question (assuming we have access to the question data)
                    var categoryName = "General"; // Default category

                    // Try to get category from question if available
                    // This would need to be populated when creating QuestionResultViewModel
                    if (!string.IsNullOrEmpty(questionResult.CategoryName))
                    {
                        categoryName = questionResult.CategoryName;
                    }

                    if (!categoryPerformance.ContainsKey(categoryName))
                    {
                        categoryPerformance[categoryName] = new CategoryPerformance
                        {
                            CategoryName = categoryName,
                            CorrectAnswers = 0,
                            TotalQuestions = 0
                        };
                    }

                    categoryPerformance[categoryName].TotalQuestions++;
                    if (questionResult.IsCorrect)
                    {
                        categoryPerformance[categoryName].CorrectAnswers++;
                    }
                }

                var summary = new TestResultSummary
                {
                    TestTitle = TestTitle,
                    ScorePercentage = (float)ScorePercentage,
                    CorrectAnswers = CorrectAnswers,
                    TotalQuestions = QuestionResults.Count,
                    MultipleChoiceCorrect = QuestionResults.Count(q => q.QuestionType == "Multiple Choice" && q.IsCorrect),
                    MultipleChoiceTotal = QuestionResults.Count(q => q.QuestionType == "Multiple Choice"),
                    ShortAnswerCorrect = QuestionResults.Count(q => q.QuestionType == "Short Answer" && q.IsCorrect),
                    ShortAnswerTotal = QuestionResults.Count(q => q.QuestionType == "Short Answer"),
                    CategoryPerformance = categoryPerformance
                };

                var insights = await _groqApiService.GenerateInsightsAsync(summary);

                // Check if insights are valid
                if (string.IsNullOrWhiteSpace(insights.Strengths) &&
                    string.IsNullOrWhiteSpace(insights.AreasToImprove) &&
                    string.IsNullOrWhiteSpace(insights.Recommendations))
                {
                    HasInsightsError = true;
                    HasAiInsights = false;
                }
                else
                {
                    AiStrengths = insights.Strengths;
                    AiAreasToImprove = insights.AreasToImprove;
                    AiRecommendations = insights.Recommendations;
                    HasAiInsights = true;
                    HasInsightsError = false;
                }

                Console.WriteLine("🤖 AI insights loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading AI insights: {ex.Message}");
                HasAiInsights = false;
                HasInsightsError = true;
            }
            finally
            {
                IsLoadingInsights = false;
            }
        }

        private async Task LoadClassPerformanceAsync(Guid classTestId)
        {
            try
            {
                Console.WriteLine("📊 Loading class performance data...");

                // Update each question with class performance data
                foreach (var questionResult in QuestionResults)
                {
                    try
                    {
                        // Get all answers for this question from all students in the class
                        var allAnswers = await _dataService.GetAnswersByClassTestAndQuestionAsync(classTestId, questionResult.QuestionId);

                        if (allAnswers.Count > 0)
                        {
                            // Calculate how many students got this question correct
                            int correctCount = 0;
                            foreach (var answer in allAnswers)
                            {
                                if (answer.Question != null && IsAnswerCorrect(answer.Question, answer))
                                {
                                    correctCount++;
                                }
                            }

                            // Calculate percentage
                            questionResult.ClassCorrectPercentage = (double)correctCount / allAnswers.Count * 100;

                            // Set performance text and color
                            if (questionResult.ClassCorrectPercentage >= 80)
                            {
                                questionResult.ClassPerformanceText = $"{questionResult.ClassCorrectPercentage:F0}% of class got this correct";
                                questionResult.ClassPerformanceColor = "#10B981"; // Green
                            }
                            else if (questionResult.ClassCorrectPercentage >= 60)
                            {
                                questionResult.ClassPerformanceText = $"{questionResult.ClassCorrectPercentage:F0}% of class got this correct";
                                questionResult.ClassPerformanceColor = "#F59E0B"; // Orange
                            }
                            else
                            {
                                questionResult.ClassPerformanceText = $"{questionResult.ClassCorrectPercentage:F0}% of class got this correct";
                                questionResult.ClassPerformanceColor = "#EF4444"; // Red
                            }

                            Console.WriteLine($"📊 Question {questionResult.QuestionNumber}: {questionResult.ClassCorrectPercentage:F1}% class correct");
                        }
                        else
                        {
                            questionResult.ClassPerformanceText = "No class data available";
                            questionResult.ClassPerformanceColor = "#6B7280";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error loading performance for question {questionResult.QuestionNumber}: {ex.Message}");
                        questionResult.ClassPerformanceText = "Performance data unavailable";
                        questionResult.ClassPerformanceColor = "#6B7280";
                    }
                }

                Console.WriteLine("✅ Class performance data loaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading class performance data: {ex.Message}");
            }
        }

        private void UpdateNavigationStates()
        {
            CanGoPrevious = CurrentAttemptNumber > 1;
            CanGoNext = CurrentAttemptNumber < TotalAttempts;
        }
    }

    public partial class QuestionResultViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _questionNumber;

        [ObservableProperty]
        private string _questionText = string.Empty;

        [ObservableProperty]
        private string _questionType = string.Empty;

        [ObservableProperty]
        private string _typeColor = "#6B7280";

        [ObservableProperty]
        private string _userAnswer = string.Empty;

        [ObservableProperty]
        private string _correctAnswer = string.Empty;

        [ObservableProperty]
        private bool _isCorrect;

        [ObservableProperty]
        private int _totalPoints;

        [ObservableProperty]
        private int _pointsEarned;

        [ObservableProperty]
        private string _statusIcon = "❌";

        [ObservableProperty]
        private string _explanation = string.Empty;

        [ObservableProperty]
        private bool _hasExplanation = false;

        [ObservableProperty]
        private bool _isLoadingExplanation = false;

        [ObservableProperty]
        private bool _showExplanation = false;

        [ObservableProperty]
        private string _aiExplanation = string.Empty;

        [ObservableProperty]
        private bool _hasExplanationError = false;

        [ObservableProperty]
        private string _difficulty = "medium";

        [ObservableProperty]
        private double _classCorrectPercentage = 0;

        [ObservableProperty]
        private string _classPerformanceText = "";

        [ObservableProperty]
        private string _classPerformanceColor = "#6B7280";

        [ObservableProperty]
        private string _categoryName = "General";

        public string[] AnswerOptions { get; set; } = Array.Empty<string>();
        public string CorrectAnswerValue { get; set; } = string.Empty;
        public string QuestionTextValue { get; set; } = string.Empty;
        public Guid QuestionId { get; set; }

        [RelayCommand]
        public async Task ExplainQuestion()
        {
            if (IsLoadingExplanation)
                return;

            if (!string.IsNullOrEmpty(AiExplanation) && !HasExplanationError)
            {
                ShowExplanation = !ShowExplanation;
                return;
            }

            await LoadExplanation();
        }

        [RelayCommand]
        public async Task RetryExplanation()
        {
            HasExplanationError = false;
            AiExplanation = string.Empty;
            await LoadExplanation();
        }

        private async Task LoadExplanation()
        {
            try
            {
                IsLoadingExplanation = true;
                HasExplanationError = false;
                var groqService = new GroqApiService();

                var explanation = await groqService.GenerateQuestionExplanationAsync(
                    QuestionTextValue,
                    AnswerOptions,
                    CorrectAnswerValue,
                    UserAnswer);

                if (explanation.Contains("Error generating explanation") ||
                    explanation.Contains("Unable to generate explanation"))
                {
                    HasExplanationError = true;
                    AiExplanation = "Failed to generate explanation. Please try again.";
                }
                else
                {
                    AiExplanation = explanation;
                    HasExplanationError = false;
                }

                ShowExplanation = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading explanation: {ex.Message}");
                HasExplanationError = true;
                AiExplanation = "Failed to generate explanation. Please try again.";
                ShowExplanation = true;
            }
            finally
            {
                IsLoadingExplanation = false;
            }
        }
    }

    public class BoolToTextColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                return "#10B981"; // Green for correct
            }
            return "#EF4444"; // Red for incorrect
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}