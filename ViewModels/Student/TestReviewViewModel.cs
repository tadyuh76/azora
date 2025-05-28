using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Services;
using AvaloniaAzora.Models;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaAzora.ViewModels.Student
{
    public partial class TestReviewViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _testTitle = string.Empty;

        [ObservableProperty]
        private int _totalQuestions = 0;

        [ObservableProperty]
        private int _answeredCount = 0;

        public ObservableCollection<QuestionReviewViewModel> Questions { get; } = new();

        // Events
        public event EventHandler? BackToTestRequested;
        public event EventHandler? SubmitTestRequested;

        public TestReviewViewModel()
        {
        }

        public void LoadQuestions(IEnumerable<QuestionViewModel> testQuestions)
        {
            Questions.Clear();

            foreach (var question in testQuestions)
            {
                var reviewQuestion = new QuestionReviewViewModel
                {
                    QuestionNumber = question.QuestionNumber,
                    QuestionText = question.Text,
                    QuestionType = question.QuestionType,
                    TypeColor = question.TypeColor,
                    Points = question.Points,
                    Difficulty = question.Difficulty,
                    IsMultipleChoice = question.IsMultipleChoice,
                    IsTrueFalse = false, // All questions are now multiple choice
                    IsShortAnswer = question.IsShortAnswer,
                    IsAnswered = question.IsAnswered,
                    StatusText = question.IsAnswered ? "Answered" : "Unanswered"
                };

                // Copy answer options for multiple choice (including converted true/false)
                if (question.IsMultipleChoice)
                {
                    foreach (var option in question.AnswerOptions)
                    {
                        reviewQuestion.AnswerOptions.Add(new AnswerOptionReviewViewModel
                        {
                            Text = option.Text,
                            IsSelected = option.IsSelected
                        });
                    }
                }

                // Copy short answer
                if (question.IsShortAnswer)
                {
                    reviewQuestion.ShortAnswerText = question.ShortAnswerText;
                }

                Questions.Add(reviewQuestion);
            }

            TotalQuestions = Questions.Count;
            AnsweredCount = Questions.Count(q => q.IsAnswered);
        }

        [RelayCommand]
        private void BackToTest()
        {
            BackToTestRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        private void SubmitTest()
        {
            SubmitTestRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    public partial class QuestionReviewViewModel : ObservableObject
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
        private int _points = 5;

        [ObservableProperty]
        private string _difficulty = "Medium";

        [ObservableProperty]
        private bool _isMultipleChoice;

        [ObservableProperty]
        private bool _isTrueFalse;

        [ObservableProperty]
        private bool _isShortAnswer;

        [ObservableProperty]
        private bool _isAnswered;

        [ObservableProperty]
        private string _statusText = "Unanswered";

        // Answer properties
        public ObservableCollection<AnswerOptionReviewViewModel> AnswerOptions { get; } = new();

        [ObservableProperty]
        private bool _trueSelected;

        [ObservableProperty]
        private bool _falseSelected;

        [ObservableProperty]
        private string _shortAnswerText = "";
    }

    public partial class AnswerOptionReviewViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _text = string.Empty;

        [ObservableProperty]
        private bool _isSelected;
    }

    // Additional converters for the review window
    public class BoolToStatusColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isAnswered && isAnswered)
            {
                return "#10B981"; // Green for answered
            }
            return "#6B7280"; // Gray for unanswered
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToAnswerColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
            {
                return "#EBF8FF"; // Light blue background for selected
            }
            return "White"; // White background for unselected
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToBorderColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isSelected && isSelected)
            {
                return "#3B82F6"; // Blue border for selected
            }
            return "#D1D5DB"; // Gray border for unselected
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}