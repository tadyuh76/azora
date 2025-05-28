using Avalonia.Controls;
using AvaloniaAzora.ViewModels.Admin;
using System;

namespace AvaloniaAzora.Views.Admin
{
    public partial class UserManagementWindow : Window
    {
        public UserManagementWindow()
        {
            InitializeComponent();
            var viewModel = new UserManagementViewModel(); // Remove parameter
            
            // Subscribe to events
            viewModel.CreateUserRequested += OnCreateUserRequested;
            viewModel.EditUserRequested += OnEditUserRequested;
            viewModel.BulkImportRequested += OnBulkImportRequested;
            
            DataContext = viewModel;
        }

        private void OnCreateUserRequested(object? sender, Guid userId)
        {
            var editWindow = new UserEditWindow();
            if (editWindow.DataContext is UserEditViewModel editViewModel)
            {
                editViewModel.SetupForCreate();
                editViewModel.UserSaved += (s, e) => 
                {
                    if (DataContext is UserManagementViewModel mainViewModel)
                    {
                        _ = mainViewModel.LoadUsersAsync();
                    }
                    editWindow.Close();
                };
                editViewModel.Cancelled += (s, e) => editWindow.Close();
            }
            editWindow.ShowDialog(this);
        }

        private async void OnEditUserRequested(object? sender, Guid userId)
        {
            var editWindow = new UserEditWindow();
            if (editWindow.DataContext is UserEditViewModel editViewModel)
            {
                await editViewModel.SetupForEditAsync(userId);
                editViewModel.UserSaved += (s, e) => 
                {
                    if (DataContext is UserManagementViewModel mainViewModel)
                    {
                        _ = mainViewModel.LoadUsersAsync();
                    }
                    editWindow.Close();
                };
                editViewModel.Cancelled += (s, e) => editWindow.Close();
            }
            editWindow.ShowDialog(this);
        }

        private void OnBulkImportRequested(object? sender, EventArgs e)
        {
            var messageBox = new Window
            {
                Title = "Bulk Import",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            
            var textBlock = new TextBlock
            {
                Text = "Bulk import functionality will be implemented here.\nSupported format: CSV with columns: Email, FullName, Role",
                Margin = new Avalonia.Thickness(20),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            
            messageBox.Content = textBlock;
            messageBox.ShowDialog(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is UserManagementViewModel viewModel)
            {
                viewModel.CreateUserRequested -= OnCreateUserRequested;
                viewModel.EditUserRequested -= OnEditUserRequested;
                viewModel.BulkImportRequested -= OnBulkImportRequested;
            }
            base.OnClosed(e);
        }
    }
}
