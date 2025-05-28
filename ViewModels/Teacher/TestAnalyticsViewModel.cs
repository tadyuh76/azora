using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaAzora.ViewModels.Teacher
{
    public partial class TestAnalyticsViewModel : ObservableObject
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private ClassTest? _classTest;

        [ObservableProperty]
        private string _testTitle = string.Empty;

        [ObservableProperty]
        private string _testDescription = string.Empty;

        [ObservableProperty]
        private int _totalStudents;

        [ObservableProperty]
        private int _totalAttempts;

        [ObservableProperty]
        private int _completedAttempts;

        [ObservableProperty]
        private double _completionRate;

        [ObservableProperty]
        private double _averageScore;

        [ObservableProperty]
        private double _averageTimeMinutes;

        [ObservableProperty]
        private ObservableCollection<AttemptViewModel> _allAttempts = new();

        [ObservableProperty]
        private ObservableCollection<TopPerformerViewModel> _topPerformers = new();

        [ObservableProperty]
        private Dictionary<string, int> _scoreDistribution = new();

        // Design-time constructor
        public TestAnalyticsViewModel()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
        }

        public TestAnalyticsViewModel(IDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task LoadDataAsync(Guid classTestId)
        {
            ClassTest = await _dataService.GetClassTestByIdAsync(classTestId);
            if (ClassTest == null) return;

            // Basic test info
            TestTitle = ClassTest.Test?.Title ?? "Unknown Test";
            TestDescription = ClassTest.Test?.Description ?? string.Empty;

            // Get enrollments to determine total students
            var enrollments = await _dataService.GetEnrollmentsByClassIdAsync(ClassTest.ClassId ?? Guid.Empty);
            TotalStudents = enrollments.Count;

            // Get all attempts for this test
            var attempts = await _dataService.GetAttemptsByClassTestIdAsync(classTestId);
            TotalAttempts = attempts.Count;

            // Only count completed attempts
            var completedAttemptsList = attempts.Where(a => a.EndTime.HasValue).ToList();
            CompletedAttempts = completedAttemptsList.Count;

            // Calculate completion rate
            CompletionRate = TotalStudents > 0
                ? Math.Round((double)CompletedAttempts / TotalStudents * 100, 1)
                : 0;

            // Calculate average score
            AverageScore = completedAttemptsList.Count > 0
                ? Math.Round(completedAttemptsList.Average(a => (double)(a.Score ?? 0)), 1)
                : 0;

            // Calculate average time spent (in minutes)
            AverageTimeMinutes = completedAttemptsList.Count > 0
                ? Math.Round(completedAttemptsList
                    .Where(a => a.StartTime != default && a.EndTime.HasValue)
                    .Average(a => (a.EndTime!.Value - a.StartTime).TotalMinutes), 1)
                : 0;

            // Calculate score distribution for pie chart
            var scoreRanges = new Dictionary<string, int>
            {
                { "0-30%", 0 },
                { "31-50%", 0 },
                { "51-70%", 0 },
                { "71-90%", 0 },
                { "91-100%", 0 }
            };

            foreach (var attempt in completedAttemptsList)
            {
                var score = attempt.Score ?? 0;
                if (score <= 30) scoreRanges["0-30%"]++;
                else if (score <= 50) scoreRanges["31-50%"]++;
                else if (score <= 70) scoreRanges["51-70%"]++;
                else if (score <= 90) scoreRanges["71-90%"]++;
                else scoreRanges["91-100%"]++;
            }

            ScoreDistribution = scoreRanges;

            // Get top performers
            TopPerformers.Clear();
            var topAttempts = completedAttemptsList
                .OrderByDescending(a => a.Score ?? 0)
                .ThenBy(a => a.EndTime!.Value - a.StartTime)
                .Take(3);

            foreach (var attempt in topAttempts)
            {
                if (attempt.StudentId.HasValue)
                {
                    var student = await _dataService.GetUserByIdAsync(attempt.StudentId.Value);
                    if (student != null)
                    {
                        TopPerformers.Add(new TopPerformerViewModel
                        {
                            StudentId = student.Id,
                            StudentName = student.FullName ?? "Unknown Student",
                            Score = (double)(attempt.Score ?? 0),
                            TimeTaken = attempt.EndTime.HasValue && attempt.StartTime != default
                                ? (attempt.EndTime.Value - attempt.StartTime).TotalMinutes
                                : 0
                        });
                    }
                }
            }

            // Load all attempts for the table
            AllAttempts.Clear();
            foreach (var attempt in attempts)
            {
                if (attempt.StudentId.HasValue)
                {
                    var student = await _dataService.GetUserByIdAsync(attempt.StudentId.Value);
                    if (student != null)
                    {
                        AllAttempts.Add(new AttemptViewModel
                        {
                            AttemptId = attempt.Id,
                            StudentId = student.Id,
                            StudentName = student.FullName ?? "Unknown Student",
                            StudentEmail = student.Email,
                            Score = (double)(attempt.Score ?? 0),
                            StartTime = attempt.StartTime,
                            EndTime = attempt.EndTime,
                            Status = attempt.EndTime.HasValue ? "Completed" : "In Progress",
                            TimeTaken = attempt.EndTime.HasValue && attempt.StartTime != default
                                ? (attempt.EndTime.Value - attempt.StartTime).TotalMinutes
                                : 0
                        });
                    }
                }
            }
        }

        [RelayCommand]
        private void SortAttemptsByName(bool ascending)
        {
            var sorted = ascending
                ? AllAttempts.OrderBy(a => a.StudentName)
                : AllAttempts.OrderByDescending(a => a.StudentName);

            UpdateSortedAttempts(sorted);
        }

        [RelayCommand]
        private void SortAttemptsByScore(bool ascending)
        {
            var sorted = ascending
                ? AllAttempts.OrderBy(a => a.Score)
                : AllAttempts.OrderByDescending(a => a.Score);

            UpdateSortedAttempts(sorted);
        }

        [RelayCommand]
        private void SortAttemptsByDate(bool ascending)
        {
            var sorted = ascending
                ? AllAttempts.OrderBy(a => a.StartTime)
                : AllAttempts.OrderByDescending(a => a.StartTime);

            UpdateSortedAttempts(sorted);
        }

        private void UpdateSortedAttempts(IEnumerable<AttemptViewModel> sorted)
        {
            AllAttempts.Clear();
            foreach (var attempt in sorted)
            {
                AllAttempts.Add(attempt);
            }
        }
    }

    public class AttemptViewModel
    {
        public Guid AttemptId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? StudentEmail { get; set; }
        public double Score { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public double TimeTaken { get; set; }
    }

    public class TopPerformerViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public double Score { get; set; }
        public double TimeTaken { get; set; }
    }
}