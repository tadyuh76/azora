using Avalonia.Controls;
using AvaloniaAzora.ViewModels.Student;
using System;

namespace AvaloniaAzora.Views.Student
{
    public partial class TestDetailWindow : Window
    {
        public TestDetailWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
        }

        public TestDetailWindow(TestDetailViewModel viewModel) : this()
        {
            DataContext = viewModel;

            // Subscribe to events from view model
            if (viewModel != null)
            {
                viewModel.GoBackRequested += OnGoBackRequested;
                viewModel.StartTestRequested += OnStartTestRequested;
                viewModel.ViewAttemptResultRequested += OnViewAttemptResultRequested;
            }
        }

        private void OnGoBackRequested(object? sender, EventArgs e)
        {
            Close();
        }

        private void OnStartTestRequested(object? sender, TestStartEventArgs e)
        {
            var testTakingWindow = new TestTakingWindow(e.ClassTestId, e.UserId);
            testTakingWindow.Show();
            Close();
        }

        private void OnViewAttemptResultRequested(object? sender, ViewAttemptResultEventArgs e)
        {
            try
            {
                Console.WriteLine($"📊 Opening test result for attempt: {e.AttemptId}");
                var resultWindow = new TestResultWindow(e.AttemptId, e.UserId);
                resultWindow.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error opening test result: {ex.Message}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is TestDetailViewModel viewModel)
            {
                viewModel.GoBackRequested -= OnGoBackRequested;
                viewModel.StartTestRequested -= OnStartTestRequested;
                viewModel.ViewAttemptResultRequested -= OnViewAttemptResultRequested;
            }
            base.OnClosed(e);
        }
    }
}