using Avalonia.Controls;
using AvaloniaAzora.ViewModels.Student;
using System;

namespace AvaloniaAzora.Views.Student
{
    public partial class TestResultWindow : Window
    {
        public TestResultWindow()
        {
            InitializeComponent();
        }

        public TestResultWindow(Guid attemptId, Guid userId) : this()
        {
            var viewModel = new TestResultViewModel();
            DataContext = viewModel;

            // Subscribe to events
            viewModel.GoBackRequested += OnGoBackRequested;

            // Load result data
            _ = viewModel.LoadResultsAsync(attemptId, userId);
        }

        private void OnGoBackRequested(object? sender, EventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is TestResultViewModel viewModel)
            {
                viewModel.GoBackRequested -= OnGoBackRequested;
            }
            base.OnClosed(e);
        }
    }
}