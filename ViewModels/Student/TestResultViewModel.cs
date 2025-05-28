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
        private string _classRank = "#8";

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

        public ObservableCollection<QuestionResultViewModel> QuestionResults { get; } = new();

        // Events
        public event EventHandler? GoBackRequested;

        public TestResultViewModel()
        {
            _dataService = (IDataService)AvaloniaAzora.Services.ServiceProvider.Instance.GetService(typeof(IDataService))!;
        }

        public async Task LoadResultsAsync(Guid attemptId, Guid userId)
        {
            try
            {
                IsLoading = true;

                Console.WriteLine($"🔍 Loading test results for attempt: {attemptId}");

                // Load attempt details
                var attempt = await _dataService.GetAttemptByIdAsync(attemptId);
                if (attempt?.ClassTest?.Test == null)
                {
                    Console.WriteLine("❌ Attempt, class test, or test not found");
                    LoadDemoResults();
                    return;
                }

                var test = attempt.ClassTest.Test;
                TestTitle = test.Title;

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
                    HasExplanation = !string.IsNullOrEmpty(GetQuestionExplanation(question, isCorrect))
                };

                QuestionResults.Add(questionResult);
            }

            // Update summary
            CorrectAnswers = correctCount;
            IncorrectAnswers = questions.Count - correctCount;
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

                case "true_false":
                    // For true/false, compare the boolean values
                    return string.Equals(userAnswer.AnswerText.Trim(), question.CorrectAnswer?.Trim(),
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

                case "true_false":
                    // Capitalize first letter
                    return userAnswer.AnswerText.ToLower() == "true" ? "True" : "False";

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

                case "true_false":
                    // Capitalize first letter
                    return question.CorrectAnswer.ToLower() == "true" ? "True" : "False";

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
                case "true_false":
                    return "Linear functions have the form y = mx + b, where m and b are constants.";
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
                "true_false" => "True/False",
                "short_answer" => "Short Answer",
                _ => "Multiple Choice"
            };
        }

        private string GetTypeColor(string? type)
        {
            return type?.ToLower() switch
            {
                "multiple_choice" => "#3B82F6",
                "true_false" => "#10B981",
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
            PointsEarned = 22;
            TotalPoints = 27;
            ClassRank = "#8";
            CompletedDateString = "January 8, 2024 at 09:38 PM";
            TimeTakenString = "38m 15s";

            QuestionResults.Clear();

            // Question 1 - Correct
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
                StatusIcon = "✅"
            });

            // Question 2 - Incorrect
            QuestionResults.Add(new QuestionResultViewModel
            {
                QuestionNumber = 2,
                QuestionText = "The equation y = 2x + 3 represents a linear function.",
                QuestionType = "True/False",
                TypeColor = "#10B981",
                UserAnswer = "False",
                CorrectAnswer = "True",
                IsCorrect = false,
                TotalPoints = 3,
                PointsEarned = 0,
                StatusIcon = "❌",
                Explanation = "Linear functions have the form y = mx + b, where m and b are constants. This equation fits that pattern.",
                HasExplanation = true
            });

            // Question 3 - Correct
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
                StatusIcon = "✅"
            });

            UpdateScoreDisplay();
        }

        [RelayCommand]
        private void GoBack()
        {
            GoBackRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void DownloadResults()
        {
            Console.WriteLine("📥 Downloading results...");
            // This would implement PDF export or similar
        }

        [RelayCommand]
        private void ViewAttempts()
        {
            Console.WriteLine("📋 Viewing all attempts...");
            // This would show a history of all test attempts
        }

        // Helper method to capitalize first letter of a string
        private string CapitalizeFirstLetter(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return char.ToUpper(text[0]) + text.Substring(1).ToLower();
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
        private bool _hasExplanation;

        [ObservableProperty]
        private string _difficulty = "medium";
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