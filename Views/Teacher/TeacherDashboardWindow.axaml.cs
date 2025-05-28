using Avalonia.Controls;
using AvaloniaAzora.ViewModels;
using System;

namespace AvaloniaAzora.Views.Teacher
{
    public partial class TeacherDashboardWindow : Window
    {
        public TeacherDashboardWindow(Guid userId)
        {
            InitializeComponent();

            var viewModel = new TeacherDashboardViewModel();
            DataContext = viewModel;

            // Load dashboard data
            _ = viewModel.LoadDashboardDataAsync(userId);
        }
    }
}