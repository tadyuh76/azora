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
    }
}