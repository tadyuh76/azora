using Avalonia.Controls;
using AvaloniaAzora.ViewModels;
using System;

namespace AvaloniaAzora.Views.Admin
{
    public partial class AdminDashboardWindow : Window
    {
        // This constructor is for design-time only
        public AdminDashboardWindow()
        {
            InitializeComponent();
        }

        public AdminDashboardWindow(Guid userId)
        {
            InitializeComponent();

            var viewModel = new AdminDashboardViewModel();
            DataContext = viewModel;

            // Subscribe to the SignOutRequested event
            viewModel.SignOutRequested += OnSignOutRequested;

            // Load dashboard data
            _ = viewModel.LoadDashboardDataAsync(userId);
        }

        private void OnSignOutRequested(object? sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("🚪 Sign out requested from admin dashboard");
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