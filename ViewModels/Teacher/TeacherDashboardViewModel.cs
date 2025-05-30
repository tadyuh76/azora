using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels
{
    public partial class TeacherDashboardViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private readonly IAuthenticationService _authService;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome back!";

        [ObservableProperty]
        private ObservableCollection<TeacherClassroomCardViewModel> _teachingClasses = new();

        [ObservableProperty]
        private ObservableCollection<RecentActivityViewModel> _recentActivities = new();

        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private string _activeClassroomsCount = "0";

        [ObservableProperty]
        private string _totalStudentsCount = "0";

        [ObservableProperty]
        private string _activeTestsCount = "0";

        [ObservableProperty]
        private string _averagePerformance = "0%";

        [ObservableProperty]
        private ObservableCollection<Class> _teachingClassesData = new();

        [ObservableProperty]
        private int _totalClasses;

        [ObservableProperty]
        private int _totalStudents;

        [ObservableProperty]
        private int _totalTests;

        public ICommand SignOutCommand { get; }

        public event EventHandler? SignOutRequested;

        public TeacherDashboardViewModel()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            _authService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IAuthenticationService>();

            SignOutCommand = new AsyncRelayCommand(SignOutAsync);
        }
        public async Task LoadDashboardDataAsync(Guid userId)
        {
            try
            {
                IsLoading = true;
                await LoadUserDataAsync(userId);
                await LoadTeachingClassesAsync(userId);
                await CalculateStatisticsAsync(userId);
                await LoadRecentActivitiesAsync(userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error loading teacher dashboard: {ex.Message}");
                WelcomeMessage = "Welcome back!";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadUserDataAsync(Guid userId)
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
                Console.WriteLine($"✅ Loaded teacher user: {displayName} (ID: {userId})");
            }
            else
            {
                Console.WriteLine($"⚠️ Teacher user not found with ID: {userId}");
                WelcomeMessage = "Welcome back!";
            }
        }

        private async Task LoadTeachingClassesAsync(Guid userId)
        {
            try
            {
                Console.WriteLine("🏫 Loading teaching classes...");
                var classes = await _dataService.GetClassesByTeacherIdAsync(userId);

                TeachingClasses.Clear();
                TeachingClassesData.Clear();
                foreach (var classEntity in classes)
                {
                    var studentCount = await _dataService.GetClassEnrollmentCountAsync(classEntity.Id);
                    var tests = await _dataService.GetTestsByClassIdAsync(classEntity.Id); var cardViewModel = new TeacherClassroomCardViewModel
                    {
                        ClassId = classEntity.Id,
                        ClassName = classEntity.ClassName,
                        Description = classEntity.Description ?? "No description available",
                        StudentCount = studentCount,
                        TestCount = tests.Count,
                        CreatedDate = classEntity.CreatedAt.ToString("MMM dd, yyyy"),
                        SubjectColor = GetSubjectColor(classEntity.ClassName) // Use className for color determination
                    };

                    TeachingClasses.Add(cardViewModel);
                    TeachingClassesData.Add(classEntity);
                }

                Console.WriteLine($"✅ Loaded {TeachingClasses.Count} teaching classes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error loading teaching classes: {ex.Message}");
            }
        }

        private async Task LoadStatisticsAsync(Guid userId)
        {
            try
            {
                var classes = await _dataService.GetClassesByTeacherIdAsync(userId);
                ActiveClassroomsCount = classes.Count.ToString();

                int totalStudents = 0;
                int totalTests = 0;

                foreach (var classEntity in classes)
                {
                    var studentCount = await _dataService.GetClassEnrollmentCountAsync(classEntity.Id);
                    var tests = await _dataService.GetTestsByClassIdAsync(classEntity.Id);

                    totalStudents += studentCount;
                    totalTests += tests.Count;
                }

                TotalStudentsCount = totalStudents.ToString();
                ActiveTestsCount = totalTests.ToString();
                AveragePerformance = "87%"; // Placeholder for now

                Console.WriteLine($"✅ Statistics loaded: {ActiveClassroomsCount} classes, {TotalStudentsCount} students, {ActiveTestsCount} tests");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error loading statistics: {ex.Message}");
                ActiveClassroomsCount = "0";
                TotalStudentsCount = "0";
                ActiveTestsCount = "0";
                AveragePerformance = "0%";
            }
        }

        private async Task LoadRecentActivitiesAsync(Guid userId)
        {
            try
            {
                // For now, add some sample activities
                // In a real implementation, you'd load actual activities from the database
                RecentActivities.Clear();

                RecentActivities.Add(new RecentActivityViewModel
                {
                    ActivityTitle = "Student submitted test",
                    ActivityDescription = "John Doe completed Algebra Quiz",
                    TimeAgo = "5 minutes ago",
                    ActivityColor = "#3B82F6"
                });

                RecentActivities.Add(new RecentActivityViewModel
                {
                    ActivityTitle = "New student enrolled",
                    ActivityDescription = "Sarah Wilson joined Mathematics 101",
                    TimeAgo = "1 hour ago",
                    ActivityColor = "#10B981"
                });

                RecentActivities.Add(new RecentActivityViewModel
                {
                    ActivityTitle = "Test created",
                    ActivityDescription = "Created new Calculus Midterm",
                    TimeAgo = "2 hours ago",
                    ActivityColor = "#F59E0B"
                });

                await Task.CompletedTask; // Remove this when implementing real data loading
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error loading recent activities: {ex.Message}");
            }
        }
        private string GetSubjectColor(string? subject)
        {
            if (string.IsNullOrEmpty(subject))
                return "#6B7280";

            return subject.ToLower() switch
            {
                var s when s.Contains("math") => "#3B82F6",      // Blue
                var s when s.Contains("science") => "#10B981",   // Green  
                var s when s.Contains("english") => "#F59E0B",   // Yellow
                var s when s.Contains("history") => "#8B5CF6",   // Purple
                var s when s.Contains("physics") => "#EF4444",   // Red
                var s when s.Contains("chemistry") => "#06B6D4", // Cyan
                var s when s.Contains("biology") => "#84CC16",   // Lime
                var s when s.Contains("geography") => "#F97316", // Orange
                var s when s.Contains("literature") => "#EC4899", // Pink
                var s when s.Contains("computer") => "#6366F1",  // Indigo
                var s when s.Contains("art") => "#A855F7",       // Violet
                var s when s.Contains("music") => "#14B8A6",     // Teal
                _ => "#6B7280"  // Default gray
            };
        }

        public async Task CalculateStatisticsAsync(Guid userId)
        {
            try
            {
                // Calculate total classes
                TotalClasses = TeachingClassesData.Count;

                // Calculate total students (deduplicated across classes)
                var studentIds = new HashSet<Guid>();
                foreach (var classItem in TeachingClassesData)
                {
                    var enrollments = await _dataService.GetEnrollmentsByClassIdAsync(classItem.Id);
                    foreach (var enrollment in enrollments)
                    {
                        if (enrollment.StudentId.HasValue)
                        {
                            studentIds.Add(enrollment.StudentId.Value);
                        }
                    }
                }
                TotalStudents = studentIds.Count;

                // Calculate total tests
                var testCount = 0;
                foreach (var classItem in TeachingClassesData)
                {
                    var tests = await _dataService.GetTestsByClassIdAsync(classItem.Id);
                    testCount += tests.Count;
                }
                TotalTests = testCount;

                // Update statistics
                ActiveClassroomsCount = TotalClasses.ToString();
                TotalStudentsCount = TotalStudents.ToString();
                ActiveTestsCount = TotalTests.ToString();
                AveragePerformance = "87%"; // Placeholder for now

                Console.WriteLine($"✅ Statistics calculated: {ActiveClassroomsCount} classes, {TotalStudentsCount} students, {ActiveTestsCount} tests");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error calculating statistics: {ex.Message}");
                ActiveClassroomsCount = "0";
                TotalStudentsCount = "0";
                ActiveTestsCount = "0";
                AveragePerformance = "0%";
            }
        }

        private async Task SignOutAsync()
        {
            try
            {
                await _authService.SignOutAsync();
                SignOutRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error signing out: {ex.Message}");
                // Even if sign out fails, we should still navigate away for security
                SignOutRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}