using Avalonia.Controls;
using AvaloniaAzora.ViewModels.Student;
using System;
using System.Threading.Tasks;

namespace AvaloniaAzora.Views.Student
{
    public partial class ClassroomDetailWindow : Window
    {
        public ClassroomDetailWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
        }

        public ClassroomDetailWindow(Guid classId, Guid userId) : this()
        {
            try
            {
                Console.WriteLine($"🏗️ Initializing ClassroomDetailWindow for class: {classId}, user: {userId}");

                var viewModel = new ClassroomDetailViewModel();
                DataContext = viewModel;

                // Handle back button click
                var backButton = this.FindControl<Button>("BackButton");
                if (backButton != null)
                {
                    backButton.Click += (sender, e) => Close();
                }

                Console.WriteLine("✅ ViewModel created and DataContext set");

                // Load data when window is opened
                Opened += async (sender, e) => await LoadDataAsync(viewModel, classId, userId);

                Console.WriteLine("✅ Opened event handler attached");

                // Subscribe to events
                viewModel.GoBackRequested += OnGoBackRequested;
                TestCardViewModel.TestStartRequested += OnTestStartRequested;
                TestCardViewModel.ViewTestRequested += OnViewTestRequested;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ClassroomDetailWindow constructor: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task LoadDataAsync(ClassroomDetailViewModel viewModel, Guid classId, Guid userId)
        {
            try
            {
                Console.WriteLine($"📚 Loading classroom details for class: {classId}, user: {userId}");
                await viewModel.LoadClassroomDetailsAsync(classId, userId);
                Console.WriteLine("✅ Classroom details loading completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading classroom details: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // You could show an error dialog here
            }
        }

        private void OnGoBackRequested(object? sender, EventArgs e)
        {
            Close();
        }

        private void OnTestStartRequested(object? sender, TestStartEventArgs e)
        {
            try
            {
                Console.WriteLine($"🎯 Starting test: {e.ClassTestId}");

                var testDetailViewModel = new TestDetailViewModel();
                var testDetailWindow = new TestDetailWindow(testDetailViewModel);

                // Load test details
                _ = testDetailViewModel.LoadTestDetailsAsync(e.ClassTestId, e.UserId);

                testDetailWindow.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error starting test: {ex.Message}");
            }
        }

        private void OnViewTestRequested(object? sender, ViewTestEventArgs e)
        {
            try
            {
                Console.WriteLine($"📋 Viewing test detail: {e.ClassTestId}");

                var testDetailViewModel = new TestDetailViewModel();
                var testDetailWindow = new TestDetailWindow(testDetailViewModel);

                // Load test details
                _ = testDetailViewModel.LoadTestDetailsAsync(e.ClassTestId, e.UserId);

                testDetailWindow.Show();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error viewing test detail: {ex.Message}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is ClassroomDetailViewModel viewModel)
            {
                viewModel.GoBackRequested -= OnGoBackRequested;
            }

            // Unsubscribe from static events
            TestCardViewModel.TestStartRequested -= OnTestStartRequested;
            TestCardViewModel.ViewTestRequested -= OnViewTestRequested;

            base.OnClosed(e);
        }
    }
}