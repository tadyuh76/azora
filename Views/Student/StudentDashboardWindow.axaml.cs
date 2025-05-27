using Avalonia.Controls;
using AvaloniaAzora.ViewModels;
using System;
using System.Threading.Tasks;

namespace AvaloniaAzora.Views
{
    public partial class StudentDashboardWindow : Window
    {
        public StudentDashboardWindow()
        {
            InitializeComponent();
        }

        public StudentDashboardWindow(Guid userId) : this()
        {
            try
            {
                Console.WriteLine($"🏗️ Initializing StudentDashboardWindow for user: {userId}");

                var viewModel = new StudentDashboardViewModel();
                DataContext = viewModel;

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
    }
}