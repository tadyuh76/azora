using Avalonia.Controls;
using AvaloniaAzora.ViewModels;
using AvaloniaAzora.Views.Admin;
using System;

namespace AvaloniaAzora.Views
{
    public partial class AdminDashboardWindow : Window
    {
        public AdminDashboardWindow()
        {
            InitializeComponent();
        }

        public AdminDashboardWindow(Guid userId) : this()
        {
            var viewModel = new AdminDashboardViewModel();
            
            // Subscribe to all navigation events
            viewModel.ManageUsersRequested += OnManageUsersRequested;
            viewModel.ManageStudentsRequested += OnManageStudentsRequested;
            viewModel.ManageTeachersRequested += OnManageTeachersRequested;
            viewModel.ManageClassesRequested += OnManageClassesRequested;
            viewModel.ManageTestsRequested += OnManageTestsRequested;
            viewModel.ManageQuestionsRequested += OnManageQuestionsRequested;
            viewModel.ManageAssessmentsRequested += OnManageAssessmentsRequested;
            viewModel.SystemSettingsRequested += OnSystemSettingsRequested;
            viewModel.ViewReportsRequested += OnViewReportsRequested;
            viewModel.ViewLogsRequested += OnViewLogsRequested;
            viewModel.ViewAnalyticsRequested += OnViewAnalyticsRequested;
            viewModel.BulkOperationsRequested += OnBulkOperationsRequested;
            
            DataContext = viewModel;
            
            // Load dashboard data
            _ = viewModel.LoadDashboardDataAsync(userId);
        }

        private void OnManageUsersRequested(object? sender, EventArgs e)
        {
            var userManagementWindow = new UserManagementWindow();
            userManagementWindow.Show();
        }

        private void OnManageStudentsRequested(object? sender, EventArgs e)
        {
            var studentManagementWindow = new StudentManagementWindow();
            studentManagementWindow.Show();
        }

        private void OnManageTeachersRequested(object? sender, EventArgs e)
        {
            var teacherManagementWindow = new TeacherManagementWindow();
            teacherManagementWindow.Show();
        }

        private void OnManageClassesRequested(object? sender, EventArgs e)
        {
            var classroomManagementWindow = new ClassroomManagementWindow();
            classroomManagementWindow.Show();
        }

        private void OnManageTestsRequested(object? sender, EventArgs e)
        {
            var testManagementWindow = new TestManagementWindow();
            testManagementWindow.Show();
        }

        private void OnManageQuestionsRequested(object? sender, EventArgs e)
        {
            // TODO: Implement Question Bank Management Window
            ShowComingSoonMessage("Question Bank Management");
        }

        private void OnManageAssessmentsRequested(object? sender, EventArgs e)
        {
            // TODO: Implement Assessment Management Window
            ShowComingSoonMessage("Assessment Management");
        }

        private void OnSystemSettingsRequested(object? sender, EventArgs e)
        {
            // TODO: Implement System Settings Window
            ShowComingSoonMessage("System Settings");
        }

        private void OnViewReportsRequested(object? sender, EventArgs e)
        {
            // TODO: Implement Reports Window
            ShowComingSoonMessage("Reports & Analytics");
        }

        private void OnViewLogsRequested(object? sender, EventArgs e)
        {
            // TODO: Implement Activity Logs Window
            ShowComingSoonMessage("Activity Logs");
        }

        private void OnViewAnalyticsRequested(object? sender, EventArgs e)
        {
            // TODO: Implement Performance Analytics Window
            ShowComingSoonMessage("Performance Analytics");
        }

        private void OnBulkOperationsRequested(object? sender, EventArgs e)
        {
            // TODO: Implement Bulk Operations Window
            ShowComingSoonMessage("Bulk Operations");
        }

        private void ShowComingSoonMessage(string featureName)
        {
            var messageWindow = new Window
            {
                Title = "Coming Soon",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            
            var stackPanel = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 16
            };
            
            var titleBlock = new TextBlock
            {
                Text = $"{featureName}",
                FontSize = 18,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            
            var messageBlock = new TextBlock
            {
                Text = "This feature is coming soon!\nImplementation is in progress.",
                TextAlignment = Avalonia.Media.TextAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            
            var okButton = new Button
            {
                Content = "OK",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Padding = new Avalonia.Thickness(20, 8)
            };
            
            okButton.Click += (s, e) => messageWindow.Close();
            
            stackPanel.Children.Add(titleBlock);
            stackPanel.Children.Add(messageBlock);
            stackPanel.Children.Add(okButton);
            
            messageWindow.Content = stackPanel;
            messageWindow.ShowDialog(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            // Unsubscribe from events to prevent memory leaks
            if (DataContext is AdminDashboardViewModel viewModel)
            {
                viewModel.ManageUsersRequested -= OnManageUsersRequested;
                viewModel.ManageStudentsRequested -= OnManageStudentsRequested;
                viewModel.ManageTeachersRequested -= OnManageTeachersRequested;
                viewModel.ManageClassesRequested -= OnManageClassesRequested;
                viewModel.ManageTestsRequested -= OnManageTestsRequested;
                viewModel.ManageQuestionsRequested -= OnManageQuestionsRequested;
                viewModel.ManageAssessmentsRequested -= OnManageAssessmentsRequested;
                viewModel.SystemSettingsRequested -= OnSystemSettingsRequested;
                viewModel.ViewReportsRequested -= OnViewReportsRequested;
                viewModel.ViewLogsRequested -= OnViewLogsRequested;
                viewModel.ViewAnalyticsRequested -= OnViewAnalyticsRequested;
                viewModel.BulkOperationsRequested -= OnBulkOperationsRequested;
            }
            
            base.OnClosed(e);
        }
    }
}
