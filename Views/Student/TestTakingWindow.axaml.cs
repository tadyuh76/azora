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
            var resultWindow = new TestResultWindow(e.AttemptId, e.UserId);
            resultWindow.Show();
            Close();
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

    public class TestCompletedEventArgs : EventArgs
    {
        public Guid AttemptId { get; set; }
        public Guid UserId { get; set; }
    }
}