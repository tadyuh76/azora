using Avalonia.Controls;
using AvaloniaAzora.ViewModels.Student;
using System;

namespace AvaloniaAzora.Views.Student
{
    public partial class TestTakingWindow : Window
    {
        private TestReviewWindow? _currentReviewWindow;

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
            viewModel.ReviewTestRequested += OnReviewTestRequested;

            // Load test data
            _ = viewModel.LoadTestAsync(classTestId, userId);
        }

        // Constructor for preview mode (teachers)
        public TestTakingWindow(Guid classTestId, Guid userId, bool isPreviewMode) : this()
        {
            var viewModel = new TestTakingViewModel();
            viewModel.IsPreviewMode = isPreviewMode;
            DataContext = viewModel;

            // Subscribe to events
            viewModel.TestCompleted += OnTestCompleted;
            viewModel.TestAborted += OnTestAborted;
            viewModel.ReviewTestRequested += OnReviewTestRequested;

            // Load test data
            _ = viewModel.LoadTestAsync(classTestId, userId);

            if (isPreviewMode)
            {
                // Update window title to indicate preview mode
                Title = Title + " - PREVIEW MODE";

                // Add preview mode indicator to the window
                if (DataContext is TestTakingViewModel vm)
                {
                    // Access the viewmodel to show preview status in UI
                    Console.WriteLine("📋 Test taking window opened in preview mode");
                }
            }
        }

        private void OnTestCompleted(object? sender, TestCompletedEventArgs e)
        {
            try
            {
                Console.WriteLine($"✅ Test completed event received - AttemptId: {e.AttemptId}, Score: {e.Score:F1}%");

                // Clean up review window if it exists
                CleanupReviewWindow();

                if (e.IsPreview)
                {
                    // Preview mode: Show a simple message and close
                    Console.WriteLine($"📋 Test preview completed! Calculated Score: {e.Score:F1}% (Preview mode - no data saved)");
                    Close();
                }
                else
                {
                    // Normal mode: Show results window
                    var resultWindow = new TestResultWindow(e.AttemptId, e.UserId);
                    Console.WriteLine("📊 Created test result window");

                    resultWindow.Show();
                    Console.WriteLine("📊 Showing test result window");

                    Close();
                    Console.WriteLine("🚪 Closed test taking window");
                }
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
            // Clean up review window if it exists
            CleanupReviewWindow();

            // Return to dashboard or previous window
            Close();
        }

        private void OnReviewTestRequested(object? sender, ReviewTestEventArgs e)
        {
            try
            {
                Console.WriteLine("📋 Opening test review window");

                // Close existing review window if any
                CleanupReviewWindow();

                var reviewViewModel = new TestReviewViewModel();
                reviewViewModel.TestTitle = e.TestTitle;
                reviewViewModel.LoadQuestions(e.Questions);

                _currentReviewWindow = new TestReviewWindow(reviewViewModel);

                // Subscribe to the test submission event from review window
                _currentReviewWindow.TestSubmissionRequested += OnReviewTestSubmissionRequested;

                _currentReviewWindow.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error showing review window: {ex.Message}");
            }
        }

        private async void OnReviewTestSubmissionRequested(object? sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("📋 Test submission requested from review window");

                // Clean up review window
                CleanupReviewWindow();

                // Submit the test through the view model
                if (DataContext is TestTakingViewModel viewModel)
                {
                    await viewModel.SubmitTestCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error submitting test from review: {ex.Message}");
            }
        }

        private void CleanupReviewWindow()
        {
            if (_currentReviewWindow != null)
            {
                _currentReviewWindow.TestSubmissionRequested -= OnReviewTestSubmissionRequested;
                try
                {
                    _currentReviewWindow.Close();
                }
                catch
                {
                    // Window might already be closed
                }
                _currentReviewWindow = null;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Clean up review window
            CleanupReviewWindow();

            if (DataContext is TestTakingViewModel viewModel)
            {
                viewModel.TestCompleted -= OnTestCompleted;
                viewModel.TestAborted -= OnTestAborted;
                viewModel.ReviewTestRequested -= OnReviewTestRequested;
                viewModel.Dispose(); // Stop timer
            }
            base.OnClosed(e);
        }
    }
}