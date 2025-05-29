using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaAzora.ViewModels
{
    public partial class StudentDashboardViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome back!";

        [ObservableProperty]
        private ObservableCollection<ClassroomCardViewModel> _enrolledClasses = new();

        [ObservableProperty]
        private ObservableCollection<UpcomingAssessmentViewModel> _upcomingAssessments = new();

        [ObservableProperty]
        private bool _isLoading = true;

        public StudentDashboardViewModel()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
        }

        public async Task LoadDashboardDataAsync(Guid userId)
        {
            try
            {
                IsLoading = true;

                // Clear any existing data
                EnrolledClasses.Clear();
                UpcomingAssessments.Clear();

                Console.WriteLine($"🔍 Loading dashboard data for user: {userId}");

                // Load real data from Supabase
                await LoadRealDataAsync(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading dashboard data: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Clear data on error and show informative message
                EnrolledClasses.Clear();
                UpcomingAssessments.Clear();
                WelcomeMessage = "Welcome back! Unable to load some data.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadRealDataAsync(Guid userId)
        {
            // Load current user
            CurrentUser = await _dataService.GetUserByIdAsync(userId);
            if (CurrentUser != null)
            {
                // Use full_name if available, otherwise use email prefix
                string displayName = !string.IsNullOrEmpty(CurrentUser.FullName)
                    ? CurrentUser.FullName
                    : CurrentUser.Email.Split('@')[0];

                WelcomeMessage = $"Welcome back, {displayName}!";
                Console.WriteLine($"✅ Loaded user: {displayName} (ID: {userId})");
            }
            else
            {
                Console.WriteLine($"⚠️ User not found with ID: {userId}");
                WelcomeMessage = "Welcome back!";
            }

            // Load enrolled classes
            Console.WriteLine("🏫 Loading enrolled classes...");
            await LoadEnrolledClassesAsync(userId);

            // Load upcoming assessments
            Console.WriteLine("📋 Loading upcoming assessments...");
            await LoadUpcomingAssessmentsAsync(userId);

            Console.WriteLine($"✅ Dashboard loaded: {EnrolledClasses.Count} classes, {UpcomingAssessments.Count} assessments");
        }



        private async Task LoadEnrolledClassesAsync(Guid userId)
        {
            try
            {
                var enrollments = await _dataService.GetClassEnrollmentsByUserIdAsync(userId);
                Console.WriteLine($"   Found {enrollments.Count} enrollments for user");

                foreach (var enrollment in enrollments)
                {
                    if (enrollment.Class != null)
                    {
                        Console.WriteLine($"   Processing class: {enrollment.Class.ClassName}");

                        var studentCount = await _dataService.GetClassEnrollmentCountAsync(enrollment.Class.Id);

                        // Handle teacher name safely - use full_name or email fallback
                        string instructorName = "Unknown Instructor";
                        if (enrollment.Class.Teacher != null)
                        {
                            instructorName = !string.IsNullOrEmpty(enrollment.Class.Teacher.FullName)
                                ? enrollment.Class.Teacher.FullName
                                : enrollment.Class.Teacher.Email?.Split('@')[0] ?? "Unknown Instructor";
                        }

                        var classroomCard = new ClassroomCardViewModel
                        {
                            ClassId = enrollment.Class.Id,
                            ClassName = enrollment.Class.ClassName,
                            Description = enrollment.Class.Description ?? "No description available",
                            StudentCount = studentCount,
                            InstructorName = instructorName,
                            SubjectColor = GetSubjectColor(enrollment.Class.ClassName)
                        };

                        // Subscribe to the ViewClass event
                        classroomCard.ViewClassRequested += OnViewClassRequested;

                        EnrolledClasses.Add(classroomCard);
                        Console.WriteLine($"   ✅ Added class: {enrollment.Class.ClassName} with {studentCount} students");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading enrolled classes: {ex.Message}");
                throw; // Re-throw to let the caller handle it
            }
        }

        private async Task LoadUpcomingAssessmentsAsync(Guid userId)
        {
            try
            {
                var enrollments = await _dataService.GetClassEnrollmentsByUserIdAsync(userId);
                Console.WriteLine($"   Checking assessments for {enrollments.Count} enrolled classes");

                foreach (var enrollment in enrollments)
                {
                    if (enrollment.Class != null)
                    {
                        var classTests = await _dataService.GetClassTestsByClassIdAsync(enrollment.Class.Id);
                        Console.WriteLine($"   Found {classTests.Count} tests for class: {enrollment.Class.ClassName}");

                        var upcomingTests = classTests.Where(ct => ct.DueDate.HasValue && ct.DueDate.Value > DateTimeOffset.UtcNow).ToList();
                        Console.WriteLine($"   {upcomingTests.Count} upcoming tests in {enrollment.Class.ClassName}");

                        foreach (var classTest in upcomingTests)
                        {
                            if (classTest.Test != null)
                            {
                                var assessment = new UpcomingAssessmentViewModel
                                {
                                    AssessmentName = classTest.Test.Title,
                                    ClassName = enrollment.Class.ClassName,
                                    DueDate = classTest.DueDate ?? DateTimeOffset.UtcNow,
                                    AssessmentType = GetAssessmentType(classTest.Test.Title)
                                };
                                UpcomingAssessments.Add(assessment);
                                Console.WriteLine($"   ✅ Added assessment: {classTest.Test.Title} due {classTest.DueDate:MMM dd}");
                            }
                        }
                    }
                }                // Sort by due date (ascending - nearest first, not reverse)
                if (UpcomingAssessments.Count > 0)
                {
                    var sorted = UpcomingAssessments.OrderBy(a => a.DueDate).Take(3).ToList();
                    UpcomingAssessments.Clear();
                    foreach (var assessment in sorted)
                    {
                        UpcomingAssessments.Add(assessment);
                    }
                    Console.WriteLine($"   📋 Sorted and limited to {sorted.Count} upcoming assessments by due date");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading upcoming assessments: {ex.Message}");
                throw; // Re-throw to let the caller handle it
            }
        }
        private string GetSubjectColor(string className)
        {
            if (string.IsNullOrEmpty(className))
                return "#757575";

            // Return hex colors based on subject - using contains for better matching
            var lowerClassName = className.ToLower();

            if (lowerClassName.Contains("math"))
                return "#4285F4"; // Blue
            if (lowerClassName.Contains("history"))
                return "#34A853"; // Green
            if (lowerClassName.Contains("chemistry"))
                return "#9C27B0"; // Purple
            if (lowerClassName.Contains("physics"))
                return "#FF9800"; // Orange
            if (lowerClassName.Contains("science"))
                return "#F44336"; // Red
            if (lowerClassName.Contains("english"))
                return "#607D8B"; // Blue Grey
            if (lowerClassName.Contains("biology"))
                return "#4CAF50"; // Green
            if (lowerClassName.Contains("geography"))
                return "#FF5722"; // Deep Orange
            if (lowerClassName.Contains("literature"))
                return "#E91E63"; // Pink
            if (lowerClassName.Contains("computer"))
                return "#2196F3"; // Light Blue
            if (lowerClassName.Contains("art"))
                return "#9C27B0"; // Purple
            if (lowerClassName.Contains("music"))
                return "#009688"; // Teal

            return "#757575"; // Default grey
        }

        private string GetAssessmentType(string testTitle)
        {
            if (testTitle.Contains("Quiz", StringComparison.OrdinalIgnoreCase))
                return "Quiz";
            if (testTitle.Contains("Midterm", StringComparison.OrdinalIgnoreCase))
                return "Midterm";
            if (testTitle.Contains("Final", StringComparison.OrdinalIgnoreCase))
                return "Final";
            if (testTitle.Contains("Lab", StringComparison.OrdinalIgnoreCase))
                return "Lab Report";
            if (testTitle.Contains("Project", StringComparison.OrdinalIgnoreCase))
                return "Project";

            return "Assessment";
        }

        private void OnViewClassRequested(Guid classId)
        {
            Console.WriteLine($"🏫 Opening classroom detail for class: {classId}");
            // This will be handled by the view to open the ClassroomDetailWindow
            ViewClassRequested?.Invoke(classId);
        }

        public event Action<Guid>? ViewClassRequested;
    }

    public partial class ClassroomCardViewModel : ObservableObject
    {
        [ObservableProperty]
        private Guid _classId;

        [ObservableProperty]
        private string _className = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private int _studentCount;

        [ObservableProperty]
        private string _instructorName = string.Empty;

        [ObservableProperty]
        private string _subjectColor = "#757575";

        public event Action<Guid>? ViewClassRequested;

        [RelayCommand]
        private void ViewClass()
        {
            Console.WriteLine($"🏫 View class requested for: {ClassName} (ID: {ClassId})");
            ViewClassRequested?.Invoke(ClassId);
        }
    }

    public partial class UpcomingAssessmentViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _assessmentName = string.Empty;

        [ObservableProperty]
        private string _className = string.Empty;

        [ObservableProperty]
        private DateTimeOffset _dueDate;

        [ObservableProperty]
        private string _assessmentType = string.Empty;

        public string DueDateString => DueDate.ToString("MMM dd");
        public string DueTimeString => DueDate.ToString("hh:mm tt");
    }
}