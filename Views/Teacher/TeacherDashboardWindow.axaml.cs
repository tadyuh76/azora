using Avalonia.Controls;
using AvaloniaAzora.ViewModels;
using System;

namespace AvaloniaAzora.Views.Teacher
{
    public partial class TeacherDashboardWindow : Window
    {
        private readonly Guid _userId;
        private readonly TeacherDashboardViewModel _viewModel;

        // This constructor is for design-time only
        public TeacherDashboardWindow()
        {
            InitializeComponent();
            _viewModel = new TeacherDashboardViewModel();
            DataContext = _viewModel;
        }

        public TeacherDashboardWindow(Guid userId)
        {
            InitializeComponent();

            _userId = userId;
            _viewModel = new TeacherDashboardViewModel();
            DataContext = _viewModel;

            // Subscribe to the SignOutRequested event
            _viewModel.SignOutRequested += OnSignOutRequested;

            // Load dashboard data
            _ = _viewModel.LoadDashboardDataAsync(_userId);
        }

        private void OnCreateClassroomClicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = new CreateClassroomWindow(_userId);
            window.Closed += (s, args) =>
            {
                _ = _viewModel.LoadDashboardDataAsync(_userId);
            };
            window.Show();
        }

        private void OnCreateTestClicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = new CreateTestWindow(_userId);
            window.Closed += (s, args) =>
            {
                _ = _viewModel.LoadDashboardDataAsync(_userId);
            };
            window.Show();
        }

        private void OnViewClassClicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Guid classId)
            {
                var window = new TeacherClassroomDetailWindow(classId, _userId);
                window.Show();
            }
        }

        private void OnSignOutRequested(object? sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("🚪 Sign out requested from teacher dashboard");
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