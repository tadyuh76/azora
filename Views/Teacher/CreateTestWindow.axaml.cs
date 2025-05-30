using Avalonia.Controls;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using AvaloniaAzora.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AvaloniaAzora.Views.Teacher
{
    public partial class CreateTestWindow : Window
    {
        private readonly Guid _creatorId;
        private readonly IDataService _dataService;
        private readonly CreateTestViewModel _viewModel;

        // Event to notify when test is created
        public event EventHandler<Guid>? TestCreated;

        // This constructor is for design-time only
        public CreateTestWindow()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            _viewModel = new CreateTestViewModel();
            DataContext = _viewModel;
            InitializeComponent();
        }

        public CreateTestWindow(Guid creatorId)
        {
            _creatorId = creatorId;
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();

            // Initialize the ViewModel first
            _viewModel = new CreateTestViewModel();
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

                var newTest = new Test
                {
                    CreatorId = _creatorId,
                    Title = _viewModel.Title.Trim(),
                    Description = _viewModel.Description?.Trim(),
                    TimeLimit = _viewModel.TimeLimit,
                    // Ensure creation date is in UTC
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var createdTest = await _dataService.CreateTestAsync(newTest);

                ShowSuccess("Test created successfully!");

                // Wait a moment to show the success message
                await Task.Delay(1000);

                // Notify that test was created
                TestCreated?.Invoke(this, createdTest.Id);

                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating test: {ex.Message}");
                ShowError($"Failed to create test: {ex.Message}");
            }
            finally
            {
                HideLoading();
            }
        }

        private bool ValidateInput()
        {
            HideMessages();

            if (string.IsNullOrWhiteSpace(_viewModel.Title))
            {
                ShowError("Please enter a test title.");
                return false;
            }

            if (_viewModel.Title.Trim().Length < 3)
            {
                ShowError("Test title must be at least 3 characters long.");
                return false;
            }

            if (_viewModel.Title.Trim().Length > 100)
            {
                ShowError("Test title cannot exceed 100 characters.");
                return false;
            }

            if (_viewModel.TimeLimit < 1)
            {
                ShowError("Time limit must be at least 1 minute.");
                return false;
            }

            if (_viewModel.TimeLimit > 480)
            {
                ShowError("Time limit cannot exceed 480 minutes (8 hours).");
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
            CreateButton.IsEnabled = false;
            CreateButton.Content = "Creating...";
        }

        private void HideLoading()
        {
            CreateButton.IsEnabled = true;
            CreateButton.Content = "Create";
        }

        private void HideMessages()
        {
            ErrorBorder.IsVisible = false;
            SuccessBorder.IsVisible = false;
        }
    }
}