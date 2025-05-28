using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaAzora.ViewModels.Admin;
using System;

namespace AvaloniaAzora.Views.Admin
{
    public partial class ClassManagementWindow : Window
    {
        public ClassManagementWindow()
        {
            AvaloniaXamlLoader.Load(this);
            var viewModel = new ClassManagementViewModel();
            
            // Subscribe to events
            viewModel.CreateClassRequested += OnCreateClassRequested;
            viewModel.EditClassRequested += OnEditClassRequested;
            viewModel.ViewEnrollmentsRequested += OnViewEnrollmentsRequested;
            
            DataContext = viewModel;
        }

        private void OnCreateClassRequested(object? sender, Guid classId)
        {
            var editWindow = new ClassEditWindow();
            if (editWindow.DataContext is ClassEditViewModel editViewModel)
            {
                editViewModel.SetupForCreate();
                editViewModel.ClassSaved += (s, e) => 
                {
                    // Refresh the class list
                    if (DataContext is ClassManagementViewModel mainViewModel)
                    {
                        _ = mainViewModel.LoadClassesAsync();
                    }
                    editWindow.Close();
                };
                editViewModel.Cancelled += (s, e) => editWindow.Close();
            }
            editWindow.ShowDialog(this);
        }

        private async void OnEditClassRequested(object? sender, Guid classId)
        {
            var editWindow = new ClassEditWindow();
            if (editWindow.DataContext is ClassEditViewModel editViewModel)
            {
                await editViewModel.SetupForEditAsync(classId);
                editViewModel.ClassSaved += (s, e) => 
                {
                    // Refresh the class list
                    if (DataContext is ClassManagementViewModel mainViewModel)
                    {
                        _ = mainViewModel.LoadClassesAsync();
                    }
                    editWindow.Close();
                };
                editViewModel.Cancelled += (s, e) => editWindow.Close();
            }
            editWindow.ShowDialog(this);
        }

        private void OnViewEnrollmentsRequested(object? sender, Guid classId)
        {
            var enrollmentWindow = new ClassEnrollmentWindow(classId);
            enrollmentWindow.ShowDialog(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is ClassManagementViewModel viewModel)
            {
                viewModel.CreateClassRequested -= OnCreateClassRequested;
                viewModel.EditClassRequested -= OnEditClassRequested;
                viewModel.ViewEnrollmentsRequested -= OnViewEnrollmentsRequested;
            }
            base.OnClosed(e);
        }
    }
}
