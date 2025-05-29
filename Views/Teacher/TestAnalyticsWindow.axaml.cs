using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using AvaloniaAzora.ViewModels.Teacher;
using Microsoft.Extensions.DependencyInjection;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaColors = Avalonia.Media.Colors;
using ScottPlotColor = ScottPlot.Color;
using ScottPlotColors = ScottPlot.Colors;

namespace AvaloniaAzora.Views.Teacher
{
    public partial class TestAnalyticsWindow : Window
    {
        private readonly TestAnalyticsViewModel _viewModel;
        private readonly ClassTest? _classTest;
        private readonly IDataService _dataService;

        public TestAnalyticsWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            _viewModel = new TestAnalyticsViewModel(_dataService);
            DataContext = _viewModel;
        }

        public TestAnalyticsWindow(ClassTest classTest)
        {
            _classTest = classTest;
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            _viewModel = new TestAnalyticsViewModel(_dataService);

            InitializeComponent();
            WindowState = WindowState.Maximized;
            DataContext = _viewModel;

            this.Opened += async (s, e) => await LoadAnalytics();
        }

        private async Task LoadAnalytics()
        {
            try
            {
                Console.WriteLine("📊 Loading test analytics...");
                if (_classTest != null)
                {
                    await _viewModel.LoadDataAsync(_classTest.Id);
                }

                // Setup charts with real data
                await CreateSubmissionsChart();
                await CreateGradeDistributionChart();

                Console.WriteLine("✅ Test analytics loaded successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading test analytics: {ex.Message}");
            }
        }

        private async Task CreateSubmissionsChart()
        {
            try
            {
                // Clear any existing plots
                SubmissionsChart.Plot.Clear();

                if (_classTest == null)
                {
                    SubmissionsChart.Plot.Add.Text("No test data available", 0, 0);
                    SubmissionsChart.Refresh();
                    return;
                }

                // Get all attempts for this test
                var attempts = await _dataService.GetAttemptsByClassTestIdAsync(_classTest.Id);
                if (attempts?.Count == 0)
                {
                    SubmissionsChart.Plot.Add.Text("No student attempts available", 0, 0);
                    SubmissionsChart.Refresh();
                    return;
                }                // Get questions for this test
                var questions = await _dataService.GetQuestionsByTestIdAsync(_classTest.Test?.Id ?? Guid.Empty);
                if (questions?.Count == 0 || questions == null)
                {
                    SubmissionsChart.Plot.Add.Text("No questions found", 0, 0);
                    SubmissionsChart.Refresh();
                    return;
                }

                // Calculate accuracy for each question
                var questionAccuracy = new List<(string QuestionText, double Accuracy, int QuestionNumber)>();

                for (int i = 0; i < questions.Count; i++)
                {
                    var question = questions[i];
                    var allAnswers = await _dataService.GetAnswersByClassTestAndQuestionAsync(_classTest.Id, question.Id);

                    if (allAnswers.Count > 0)
                    {
                        int correctCount = 0;
                        foreach (var answer in allAnswers)
                        {
                            if (IsAnswerCorrect(question, answer))
                            {
                                correctCount++;
                            }
                        }
                        double accuracy = (double)correctCount / allAnswers.Count * 100;
                        var questionText = !string.IsNullOrEmpty(question.Text) && question.Text.Length > 40
                            ? question.Text.Substring(0, 37) + "..."
                            : question.Text ?? "Question text not available";

                        questionAccuracy.Add((questionText, accuracy, i + 1));
                    }
                }

                // Sort by accuracy (hardest questions first)
                var hardestQuestions = questionAccuracy.OrderBy(x => x.Accuracy).Take(Math.Min(6, questionAccuracy.Count)).ToList();

                if (hardestQuestions.Count == 0)
                {
                    SubmissionsChart.Plot.Add.Text("No question data available", 0, 0);
                    SubmissionsChart.Refresh();
                    return;
                }

                // Create bar chart
                var positions = hardestQuestions.Select((_, index) => (double)index).ToArray();
                var accuracies = hardestQuestions.Select(x => x.Accuracy).ToArray();
                var labels = hardestQuestions.Select(x => $"Q{x.QuestionNumber}").ToArray();

                var bar = SubmissionsChart.Plot.Add.Bars(positions, accuracies);

                // Color bars based on difficulty (red = hardest, green = easiest)
                for (int i = 0; i < bar.Bars.Count; i++)
                {
                    var accuracy = accuracies[i];
                    if (accuracy < 30)
                        bar.Bars[i].FillColor = ScottPlotColor.FromHex("#EF4444"); // Red
                    else if (accuracy < 50)
                        bar.Bars[i].FillColor = ScottPlotColor.FromHex("#F59E0B"); // Orange
                    else if (accuracy < 70)
                        bar.Bars[i].FillColor = ScottPlotColor.FromHex("#F59E0B"); // Orange
                    else
                        bar.Bars[i].FillColor = ScottPlotColor.FromHex("#10B981"); // Green
                }

                // Customize the plot
                SubmissionsChart.Plot.Axes.Title.Label.Text = "Hardest Questions by Student Accuracy";
                SubmissionsChart.Plot.Axes.Title.Label.FontSize = 16;
                SubmissionsChart.Plot.Axes.Title.Label.FontName = "Segoe UI";
                SubmissionsChart.Plot.Axes.Title.Label.Bold = true;

                SubmissionsChart.Plot.Axes.Bottom.Label.Text = "Questions";
                SubmissionsChart.Plot.Axes.Bottom.Label.FontSize = 12;
                SubmissionsChart.Plot.Axes.Bottom.SetTicks(positions, labels);

                SubmissionsChart.Plot.Axes.Left.Label.Text = "Student Accuracy (%)";
                SubmissionsChart.Plot.Axes.Left.Label.FontSize = 12;
                SubmissionsChart.Plot.Axes.Left.Min = 0;
                SubmissionsChart.Plot.Axes.Left.Max = 100;

                // Add grid
                SubmissionsChart.Plot.Grid.MajorLineColor = ScottPlotColor.FromHex("#E5E7EB");
                SubmissionsChart.Plot.Grid.MajorLineWidth = 1;

                // Set background colors
                SubmissionsChart.Plot.FigureBackground.Color = ScottPlotColors.White;
                SubmissionsChart.Plot.DataBackground.Color = ScottPlotColors.White;

                // Add value labels on top of bars
                for (int i = 0; i < positions.Length; i++)
                {
                    var text = SubmissionsChart.Plot.Add.Text($"{accuracies[i]:F0}%", positions[i], accuracies[i] + 2);
                    text.LabelFontSize = 10;
                    text.LabelFontColor = ScottPlotColor.FromHex("#374151");
                    text.LabelAlignment = ScottPlot.Alignment.MiddleCenter;
                }

                SubmissionsChart.Refresh();
                Console.WriteLine($"✅ Created hardest questions chart with {hardestQuestions.Count} questions");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating hardest questions chart: {ex.Message}");
                SubmissionsChart.Plot.Clear();
                SubmissionsChart.Plot.Add.Text("Error loading question data", 0, 0);
                SubmissionsChart.Refresh();
            }
        }

        private bool IsAnswerCorrect(Question question, UserAnswer userAnswer)
        {
            if (userAnswer == null || string.IsNullOrEmpty(userAnswer.AnswerText))
                return false;

            switch (question.Type?.ToLower())
            {
                case "multiple_choice":
                    return string.Equals(userAnswer.AnswerText, question.CorrectAnswer?.Trim(),
                        StringComparison.OrdinalIgnoreCase);

                case "short_answer":
                    return string.Equals(
                        userAnswer.AnswerText.Trim().ToLowerInvariant(),
                        question.CorrectAnswer?.Trim().ToLowerInvariant(),
                        StringComparison.OrdinalIgnoreCase);

                default:
                    return false;
            }
        }

        private async Task CreateGradeDistributionChart()
        {
            try
            {
                // Clear any existing plots
                GradeDistributionChart.Plot.Clear();

                if (_classTest == null)
                {
                    GradeDistributionChart.Plot.Add.Text("No test data available", 0, 0);
                    GradeDistributionChart.Refresh();
                    return;
                }                // Get all attempts for this test
                var attempts = await _dataService.GetAttemptsByClassTestIdAsync(_classTest.Id);
                if (attempts?.Count == 0 || attempts == null)
                {
                    GradeDistributionChart.Plot.Add.Text("No student attempts available", 0, 0);
                    GradeDistributionChart.Refresh();
                    return;
                }

                // Get unique attempts per student (best score)
                var studentBestAttempts = attempts
                    .Where(a => a.Score.HasValue)
                    .GroupBy(a => a.StudentId)
                    .Select(g => g.OrderByDescending(a => a.Score).First())
                    .ToList();

                if (studentBestAttempts.Count == 0)
                {
                    GradeDistributionChart.Plot.Add.Text("No completed attempts", 0, 0);
                    GradeDistributionChart.Refresh();
                    return;
                }

                // Define grade thresholds and labels
                var gradeRanges = new[]
                {
                    (Min: 90.0, Max: 100.0, Label: "A+ (90-100%)", Color: "#10B981"),
                    (Min: 80.0, Max: 89.9, Label: "A (80-89%)", Color: "#22C55E"),
                    (Min: 70.0, Max: 79.9, Label: "B+ (70-79%)", Color: "#3B82F6"),
                    (Min: 60.0, Max: 69.9, Label: "B (60-69%)", Color: "#6366F1"),
                    (Min: 50.0, Max: 59.9, Label: "C (50-59%)", Color: "#F59E0B"),
                    (Min: 0.0, Max: 49.9, Label: "F (0-49%)", Color: "#EF4444")
                };

                // Count students in each grade range
                var gradeCounts = new List<double>();
                var gradeLabels = new List<string>();
                var gradeColors = new List<ScottPlotColor>();

                foreach (var range in gradeRanges)
                {
                    var count = studentBestAttempts.Count(a =>
                        a.Score >= range.Min && a.Score <= range.Max);

                    if (count > 0) // Only include grades that have students
                    {
                        gradeCounts.Add(count);
                        gradeLabels.Add(range.Label);
                        gradeColors.Add(ScottPlotColor.FromHex(range.Color));
                    }
                }

                if (gradeCounts.Count == 0)
                {
                    GradeDistributionChart.Plot.Add.Text("No valid scores found", 0, 0);
                    GradeDistributionChart.Refresh();
                    return;
                }

                // Create pie chart
                var pie = GradeDistributionChart.Plot.Add.Pie(gradeCounts.ToArray());

                // Set colors for each slice
                for (int i = 0; i < pie.Slices.Count && i < gradeColors.Count; i++)
                {
                    pie.Slices[i].FillColor = gradeColors[i];
                }

                // Add legend
                var legend = GradeDistributionChart.Plot.Legend;
                legend.IsVisible = true;
                legend.Alignment = ScottPlot.Alignment.MiddleRight;
                legend.FontSize = 11;
                legend.FontColor = ScottPlotColor.FromHex("#374151");
                legend.BackgroundColor = ScottPlotColors.White;
                legend.OutlineColor = ScottPlotColor.FromHex("#D1D5DB");
                legend.ShadowColor = ScottPlotColor.FromHex("#00000020");
                legend.Padding = new ScottPlot.PixelPadding(8);

                // Add legend entries
                for (int i = 0; i < gradeLabels.Count; i++)
                {
                    var count = (int)gradeCounts[i];
                    var percentage = (count * 100.0 / studentBestAttempts.Count);
                    var legendLabel = $"{gradeLabels[i]} ({count} students, {percentage:F1}%)";

                    // Create a dummy scatter plot for legend entry
                    var scatter = GradeDistributionChart.Plot.Add.Scatter(new double[] { }, new double[] { });
                    scatter.LegendText = legendLabel;
                    scatter.Color = gradeColors[i];
                    scatter.MarkerSize = 8;
                    scatter.LineWidth = 0;
                }

                // Customize the plot
                GradeDistributionChart.Plot.Axes.Title.Label.Text = "Grade Distribution";
                GradeDistributionChart.Plot.Axes.Title.Label.FontSize = 16;
                GradeDistributionChart.Plot.Axes.Title.Label.FontName = "Segoe UI";
                GradeDistributionChart.Plot.Axes.Title.Label.Bold = true;

                // Hide axes for pie chart
                GradeDistributionChart.Plot.Axes.Frameless();
                GradeDistributionChart.Plot.HideGrid();

                // Set background colors
                GradeDistributionChart.Plot.FigureBackground.Color = ScottPlotColors.White;
                GradeDistributionChart.Plot.DataBackground.Color = ScottPlotColors.White;

                // Adjust margins to make pie chart bigger and accommodate legend
                GradeDistributionChart.Plot.Axes.Margins(0.05, 0.45, 0.05, 0.15); // left, right, bottom, top - increased top and right margins

                GradeDistributionChart.Refresh();
                Console.WriteLine($"✅ Created grade distribution chart with {gradeCounts.Count} grade ranges");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating grade distribution chart: {ex.Message}");
                GradeDistributionChart.Plot.Clear();
                GradeDistributionChart.Plot.Add.Text("Error loading grade data", 0, 0);
                GradeDistributionChart.Refresh();
            }
        }

        private void OnBackClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        private async void OnRefreshClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            await LoadAnalytics();
        }

        private async void OnWindowOpened(object? sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("📊 TestAnalyticsWindow opened, setting up charts...");
                await LoadAnalytics();
                Console.WriteLine("✅ Charts setup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error setting up charts: {ex.Message}");
            }
        }
    }

    // Value converters for UI styling
    public class StringToInitialConverter : IValueConverter
    {
        public static readonly StringToInitialConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrEmpty(str))
            {
                return str.Substring(0, 1).ToUpper();
            }
            return "?";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ScoreToColorConverter : IMultiValueConverter
    {
        public static readonly ScoreToColorConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values[0] is double score)
            {
                if (score >= 90) return Brushes.Green;
                if (score >= 80) return Brushes.LimeGreen;
                if (score >= 70) return Brushes.Orange;
                if (score >= 60) return Brushes.OrangeRed;
                return Brushes.Red;
            }
            return Brushes.Gray;
        }
    }

    public class StatusToColorConverter : IMultiValueConverter
    {
        public static readonly StatusToColorConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values[0] is string status)
            {
                return status.ToLower() switch
                {
                    "completed" => Brushes.Green,
                    "in progress" => Brushes.Orange,
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }
    }
}