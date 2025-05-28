using Avalonia.Controls;
using AvaloniaAzora.ViewModels;
using System;

namespace AvaloniaAzora.Views.Admin
{
    public partial class AdminDashboardWindow : Window
    {
        public AdminDashboardWindow(Guid userId)
        {
            InitializeComponent();

            var viewModel = new AdminDashboardViewModel();
            DataContext = viewModel;

            // Load dashboard data
            _ = viewModel.LoadDashboardDataAsync(userId);
        }
    }
}