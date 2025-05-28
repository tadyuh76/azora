using Avalonia.Controls;
using AvaloniaAzora.ViewModels;
using AvaloniaAzora.Views.Admin;
using System;
using System.Threading.Tasks;

namespace AvaloniaAzora.Views.Admin
{
    public partial class AdminDashboardWindow : Window
    {
        private readonly Guid _adminUserId;

        public AdminDashboardWindow(Guid adminUserId)
        {
            InitializeComponent();
            _adminUserId = adminUserId;
            
            var viewModel = new AdminDashboardViewModel();
            
            // Subscribe to navigation events
            viewModel.ManageUsersRequested += OnManageUsersRequested;
            viewModel.ManageClassesRequested += OnManageClassesRequested;
            viewModel.SystemSettingsRequested += OnSystemSettingsRequested;
            viewModel.ViewReportsRequested += OnViewReportsRequested;
            
            DataContext = viewModel;
            
            // Load dashboard data
            _ = LoadDashboardAsync();
        }

        private async Task LoadDashboardAsync()
        {
            if (DataContext is AdminDashboardViewModel viewModel)
            {
                await viewModel.LoadDashboardDataAsync(_adminUserId);
            }
        }

        private void OnManageUsersRequested(object? sender, EventArgs e)
        {
            var userManagementWindow = new UserManagementWindow();
            userManagementWindow.Show();
        }

        private void OnManageClassesRequested(object? sender, EventArgs e)
        {
            // TODO: Implement class management window
            ShowNotImplementedMessage("Class Management");
        }

        private void OnSystemSettingsRequested(object? sender, EventArgs e)
        {
            // TODO: Implement system settings window
            ShowNotImplementedMessage("System Settings");
        }

        private void OnViewReportsRequested(object? sender, EventArgs e)
        {
            // TODO: Implement reports window
            ShowNotImplementedMessage("Reports & Analytics");
        }

        private void ShowNotImplementedMessage(string feature)
        {
            var messageBox = new Window
            {
                Title = feature,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            
            var textBlock = new TextBlock
            {
                Text = $"{feature} functionality will be implemented in the next phase.",
                Margin = new Avalonia.Thickness(20),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            
            messageBox.Content = textBlock;
            messageBox.ShowDialog(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is AdminDashboardViewModel viewModel)
            {
                viewModel.ManageUsersRequested -= OnManageUsersRequested;
                viewModel.ManageClassesRequested -= OnManageClassesRequested;
                viewModel.SystemSettingsRequested -= OnSystemSettingsRequested;
                viewModel.ViewReportsRequested -= OnViewReportsRequested;
            }
            base.OnClosed(e);
        }
    }
}
