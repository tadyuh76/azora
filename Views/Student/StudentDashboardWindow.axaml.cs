using Avalonia.Controls;
using AvaloniaAzora.ViewModels;
using AvaloniaAzora.Views.Student;
using System;
using System.Threading.Tasks;

namespace AvaloniaAzora.Views
{
    public partial class StudentDashboardWindow : Window
    {
        private Guid _userId;

        public StudentDashboardWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
        }

        public StudentDashboardWindow(Guid userId) : this()
        {
            try
            {
                _userId = userId;
                Console.WriteLine($"🏗️ Initializing StudentDashboardWindow for user: {userId}");

                var viewModel = new StudentDashboardViewModel();
                DataContext = viewModel;

                // Subscribe to the ViewClassRequested event
                viewModel.ViewClassRequested += OnViewClassRequested;

                // Subscribe to the SignOutRequested event
                viewModel.SignOutRequested += OnSignOutRequested;

                Console.WriteLine("✅ ViewModel created and DataContext set");

                // Load data when window is opened
                Opened += async (sender, e) => await LoadDataAsync(viewModel, userId);

                Console.WriteLine("✅ Opened event handler attached");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in StudentDashboardWindow constructor: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task LoadDataAsync(StudentDashboardViewModel viewModel, Guid userId)
        {
            try
            {
                Console.WriteLine($"📚 Loading dashboard data for user: {userId}");
                await viewModel.LoadDashboardDataAsync(userId);
                Console.WriteLine("✅ Dashboard data loading completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading dashboard data: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // You could show an error dialog here
            }
        }

        private void OnViewClassRequested(Guid classId)
        {
            try
            {
                Console.WriteLine($"🏫 Opening classroom detail window for class: {classId}");
                var classroomDetailWindow = new ClassroomDetailWindow(classId, _userId);
                classroomDetailWindow.Show();
                Console.WriteLine("✅ Classroom detail window opened successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error opening classroom detail window: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private void OnSignOutRequested(object? sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("🚪 Sign out requested from student dashboard");
                // Close this window which will trigger the Closed event and show auth window
                this.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error handling sign out: {ex.Message}");
            }
        }
    }
}