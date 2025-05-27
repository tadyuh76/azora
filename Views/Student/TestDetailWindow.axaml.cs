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

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is TestDetailViewModel viewModel)
            {
                viewModel.GoBackRequested -= OnGoBackRequested;
                viewModel.StartTestRequested -= OnStartTestRequested;
            }
            base.OnClosed(e);
        }
    }

    public class TestStartEventArgs : EventArgs
    {
        public Guid ClassTestId { get; set; }
        public Guid UserId { get; set; }
    }
}