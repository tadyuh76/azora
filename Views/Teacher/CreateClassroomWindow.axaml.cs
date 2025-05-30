using Avalonia.Controls;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using AvaloniaAzora.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AvaloniaAzora.Views.Teacher
{
    public partial class CreateClassroomWindow : Window
    {
        private readonly Guid _teacherId;
        private readonly IDataService _dataService;
        private readonly CreateClassroomViewModel _viewModel;

        // Event to signal when a classroom is successfully created
        public event EventHandler<ClassroomCreatedEventArgs>? ClassroomCreated;

        // Property to track if a classroom was created
        public bool WasClassroomCreated { get; private set; } = false;

        // This constructor is for design-time only
        public CreateClassroomWindow()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            _viewModel = new CreateClassroomViewModel();
            DataContext = _viewModel;
            InitializeComponent();
        }

        public CreateClassroomWindow(Guid teacherId)
        {
            _teacherId = teacherId;
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();

            // Initialize the ViewModel first
            _viewModel = new CreateClassroomViewModel();
            DataContext = _viewModel;

            InitializeComponent();
        }

        private void OnCancelClicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        private async void OnCreateClicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                ShowLoading();

                var newClass = new Class
                {
                    TeacherId = _teacherId,
                    ClassName = _viewModel.ClassName.Trim(),
                    Description = _viewModel.Description?.Trim(),
                    // Ensure creation date is in UTC
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var createdClass = await _dataService.CreateClassAsync(newClass);
                WasClassroomCreated = true;

                ShowSuccess("Classroom created successfully!");

                // Wait a moment to show the success message
                await Task.Delay(1000);

                // Raise the ClassroomCreated event
                ClassroomCreated?.Invoke(this, new ClassroomCreatedEventArgs { ClassroomId = createdClass.Id });

                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating classroom: {ex.Message}");
                ShowError($"Failed to create classroom: {ex.Message}");
            }
            finally
            {
                HideLoading();
            }
        }

        private bool ValidateInput()
        {
            HideMessages();

            if (string.IsNullOrWhiteSpace(_viewModel.ClassName))
            {
                ShowError("Please enter a class name.");
                return false;
            }

            if (_viewModel.ClassName.Trim().Length < 3)
            {
                ShowError("Class name must be at least 3 characters long.");
                return false;
            }

            if (_viewModel.ClassName.Trim().Length > 100)
            {
                ShowError("Class name cannot exceed 100 characters.");
                return false;
            }

            return true;
        }

        private void ShowError(string message)
        {
            HideMessages();
            ErrorText.Text = message;
            ErrorBorder.IsVisible = true;
        }

        private void ShowSuccess(string message)
        {
            HideMessages();
            SuccessText.Text = message;
            SuccessBorder.IsVisible = true;
        }

        private void ShowLoading()
        {
            // Disable the create button during loading
            var createButton = this.FindControl<Button>("CreateButton");
            if (createButton != null)
            {
                createButton.IsEnabled = false;
                createButton.Content = "Creating...";
            }
        }

        private void HideLoading()
        {
            // Re-enable the create button
            var createButton = this.FindControl<Button>("CreateButton");
            if (createButton != null)
            {
                createButton.IsEnabled = true;
                createButton.Content = "Create";
            }
        }

        private void HideMessages()
        {
            ErrorBorder.IsVisible = false;
            SuccessBorder.IsVisible = false;
        }
    }

    public class ClassroomCreatedEventArgs : EventArgs
    {
        public Guid ClassroomId { get; set; }
    }
}