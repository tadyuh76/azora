using Avalonia.Controls;
using AvaloniaAzora.ViewModels.Student;
using System;

namespace AvaloniaAzora.Views.Student
{
    public partial class TestTakingWindow : Window
    {
        public TestTakingWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
        }

        public TestTakingWindow(Guid classTestId, Guid userId) : this()
        {
            var viewModel = new TestTakingViewModel();
            DataContext = viewModel;

            // Subscribe to events
            viewModel.TestCompleted += OnTestCompleted;
            viewModel.TestAborted += OnTestAborted;

            // Load test data
            _ = viewModel.LoadTestAsync(classTestId, userId);
        }

        private void OnTestCompleted(object? sender, TestCompletedEventArgs e)
        {
            try
            {
                Console.WriteLine($"✅ Test completed event received - AttemptId: {e.AttemptId}, Score: {e.Score:F1}%");

                var resultWindow = new TestResultWindow(e.AttemptId, e.UserId);
                Console.WriteLine("📊 Created test result window");

                resultWindow.Show();
                Console.WriteLine("📊 Showing test result window");

                Close();
                Console.WriteLine("🚪 Closed test taking window");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error showing test result window: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Still close the test window even if result window fails
                Close();
            }
        }

        private void OnTestAborted(object? sender, EventArgs e)
        {
            // Return to dashboard or previous window
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is TestTakingViewModel viewModel)
            {
                viewModel.TestCompleted -= OnTestCompleted;
                viewModel.TestAborted -= OnTestAborted;
                viewModel.Dispose(); // Stop timer
            }
            base.OnClosed(e);
        }
    }
}