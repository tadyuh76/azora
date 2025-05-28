using CommunityToolkit.Mvvm.ComponentModel;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaAzora.ViewModels.Student
{
    public partial class ClassroomDetailViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private Class? _currentClass;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private string _className = string.Empty;

        [ObservableProperty]
        private string _classDescription = string.Empty;

        [ObservableProperty]
        private string _instructorName = string.Empty;

        [ObservableProperty]
        private string _instructorEmail = string.Empty;

        [ObservableProperty]
        private string _schedule = "Mon, Wed, Fri - 10:00 AM";

        [ObservableProperty]
        private string _location = "Room 204, Science Building";

        [ObservableProperty]
        private int _studentCount;

        [ObservableProperty]
        private ObservableCollection<TestCardViewModel> _upcomingTests = new();

        [ObservableProperty]
        private ObservableCollection<TestCardViewModel> _completedTests = new();

        [ObservableProperty]
        private bool _isLoading = true;

        public ClassroomDetailViewModel()
        {
            _dataService = (IDataService)AvaloniaAzora.Services.ServiceProvider.Instance.GetService(typeof(IDataService))!;
        }

        public async Task LoadClassroomDetailsAsync(Guid classId, Guid userId)
        {
            try
            {
                IsLoading = true;
                Console.WriteLine($"🔍 Loading classroom details for class: {classId}, user: {userId}");

                // Load current user
                CurrentUser = await _dataService.GetUserByIdAsync(userId);

                // Load class details
                CurrentClass = await _dataService.GetClassByIdAsync(classId);
                if (CurrentClass?.Teacher != null)
                {
                    // Load full teacher details
                    var teacher = await _dataService.GetUserByIdAsync(CurrentClass.Teacher.Id);
                    if (teacher != null)
                    {
                        CurrentClass.Teacher = teacher;
                    }
                }

                if (CurrentClass != null)
                {
                    ClassName = CurrentClass.ClassName;
                    ClassDescription = CurrentClass.Description ?? "Algebra, Linear Equations, and Quadratic Functions";

                    if (CurrentClass.Teacher != null)
                    {
                        InstructorName = !string.IsNullOrEmpty(CurrentClass.Teacher.FullName)
                            ? CurrentClass.Teacher.FullName
                            : CurrentClass.Teacher.Email?.Split('@')[0] ?? "Unknown Instructor";
                        InstructorEmail = CurrentClass.Teacher.Email ?? "";
                    }

                    StudentCount = await _dataService.GetClassEnrollmentCountAsync(classId);

                    // Load tests for this class
                    await LoadClassTestsAsync(classId, userId);
                }

                Console.WriteLine($"✅ Classroom details loaded: {ClassName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading classroom details: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadClassTestsAsync(Guid classId, Guid userId)
        {
            try
            {
                var classTests = await _dataService.GetClassTestsByClassIdAsync(classId);
                Console.WriteLine($"📝 Found {classTests.Count} tests for class");

                UpcomingTests.Clear();
                CompletedTests.Clear();

                foreach (var classTest in classTests)
                {
                    if (classTest.Test != null)
                    {
                        // Get question count
                        var questions = await _dataService.GetQuestionsByTestIdAsync(classTest.Test.Id);
                        int questionCount = questions.Count;

                        // Get attempt count for this user
                        var allAttempts = await _dataService.GetAttemptsByClassTestIdAsync(classTest.Id);
                        var userAttempts = allAttempts.Where(a => a.StudentId == userId).ToList();
                        int attemptCount = userAttempts.Count;

                        var testCard = new TestCardViewModel
                        {
                            ClassTestId = classTest.Id,
                            TestId = classTest.Test.Id,
                            TestName = classTest.Test.Title,
                            TestType = GetTestType(classTest.Test.Title),
                            DueDate = classTest.DueDate ?? DateTimeOffset.Now,
                            QuestionCount = questionCount,
                            TimeLimit = classTest.Test.TimeLimit ?? 45, // Default 45 minutes
                            AttemptCount = attemptCount,
                            MaxAttempts = classTest.LimitAttempts ?? 2,
                            IsCompleted = userAttempts.Any(a => a.Score.HasValue) // Has at least one scored attempt
                        };

                        if (testCard.IsCompleted || (classTest.DueDate.HasValue && classTest.DueDate.Value < DateTimeOffset.Now))
                        {
                            CompletedTests.Add(testCard);
                        }
                        else
                        {
                            UpcomingTests.Add(testCard);
                        }
                    }
                }

                // Sort tests by due date
                var sortedUpcoming = UpcomingTests.OrderBy(t => t.DueDate).ToList();
                var sortedCompleted = CompletedTests.OrderByDescending(t => t.DueDate).ToList();

                UpcomingTests.Clear();
                CompletedTests.Clear();

                foreach (var test in sortedUpcoming)
                    UpcomingTests.Add(test);

                foreach (var test in sortedCompleted)
                    CompletedTests.Add(test);

                Console.WriteLine($"📊 Tests categorized: {UpcomingTests.Count} upcoming, {CompletedTests.Count} completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading class tests: {ex.Message}");
                throw;
            }
        }

        private string GetTestType(string testTitle)
        {
            if (testTitle.Contains("Quiz", StringComparison.OrdinalIgnoreCase))
                return "quiz";
            if (testTitle.Contains("Midterm", StringComparison.OrdinalIgnoreCase))
                return "exam";
            if (testTitle.Contains("Final", StringComparison.OrdinalIgnoreCase))
                return "exam";
            if (testTitle.Contains("Exam", StringComparison.OrdinalIgnoreCase))
                return "exam";

            return "quiz"; // Default to quiz
        }

        [RelayCommand]
        private void GoBack()
        {
            // This will be handled by the view to close the window
            Console.WriteLine("🔙 Going back to dashboard");
        }
    }

    public partial class TestCardViewModel : ObservableObject
    {
        [ObservableProperty]
        private Guid _classTestId;

        [ObservableProperty]
        private Guid _testId;

        [ObservableProperty]
        private string _testName = string.Empty;

        [ObservableProperty]
        private string _testType = "quiz"; // "quiz" or "exam"

        [ObservableProperty]
        private DateTimeOffset _dueDate;

        [ObservableProperty]
        private int _questionCount;

        [ObservableProperty]
        private int _timeLimit; // in minutes

        [ObservableProperty]
        private int _attemptCount;

        [ObservableProperty]
        private int _maxAttempts;

        [ObservableProperty]
        private bool _isCompleted;

        public string DueDateString => DueDate.ToString("MMM dd, hh:mm tt");
        public string StatusText => IsCompleted ? "Completed" : "Upcoming";
        public string AttemptText => $"{AttemptCount}/{MaxAttempts} attempts";

        [RelayCommand]
        private void StartTest()
        {
            Console.WriteLine($"🎯 Opening test details: {TestName}");

            // Open test detail window
            var testDetailViewModel = new TestDetailViewModel();
            var testDetailWindow = new Views.Student.TestDetailWindow(testDetailViewModel);

            // Get the current user ID from the parent - we need to pass it down
            // For now, we'll retrieve it from the authentication service
            var authService = (AvaloniaAzora.Services.IAuthenticationService)AvaloniaAzora.Services.ServiceProvider.Instance.GetService(typeof(AvaloniaAzora.Services.IAuthenticationService))!;
            var currentUser = authService.GetCurrentUser();

            // Safely parse the user ID with null check
            var userId = currentUser?.Id != null ? Guid.Parse(currentUser.Id) : Guid.NewGuid();

            // Load test details
            _ = testDetailViewModel.LoadTestDetailsAsync(ClassTestId, userId);

            testDetailWindow.Show();
        }
    }
}