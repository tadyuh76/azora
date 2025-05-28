using Avalonia.Controls;
using AvaloniaAzora.ViewModels.Student;
using System;

namespace AvaloniaAzora.Views.Student
{
    public partial class TestReviewWindow : Window
    {
        // Event to notify when test should be submitted
        public event EventHandler? TestSubmissionRequested;

        public TestReviewWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
        }

        public TestReviewWindow(TestReviewViewModel viewModel) : this()
        {
            DataContext = viewModel;

            // Subscribe to events
            if (viewModel != null)
            {
                viewModel.BackToTestRequested += OnBackToTestRequested;
                viewModel.SubmitTestRequested += OnSubmitTestRequested;
            }
        }

        private void OnBackToTestRequested(object? sender, EventArgs e)
        {
            Close();
        }

        private void OnSubmitTestRequested(object? sender, EventArgs e)
        {
            // Notify that test should be submitted
            TestSubmissionRequested?.Invoke(this, EventArgs.Empty);
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is TestReviewViewModel viewModel)
            {
                viewModel.BackToTestRequested -= OnBackToTestRequested;
                viewModel.SubmitTestRequested -= OnSubmitTestRequested;
            }
            base.OnClosed(e);
        }
    }
}