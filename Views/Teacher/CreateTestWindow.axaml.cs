using Avalonia.Controls;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using AvaloniaAzora.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AvaloniaAzora.Views.Teacher
{
    public partial class CreateTestWindow : Window
    {
        private readonly Guid _creatorId;
        private readonly IDataService _dataService;
        private readonly CreateTestViewModel _viewModel;

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
            var title = _viewModel.Title;
            if (string.IsNullOrWhiteSpace(title))
                return;

            var newTest = new Test
            {
                CreatorId = _creatorId,
                Title = title,
                Description = _viewModel.Description,
                TimeLimit = _viewModel.TimeLimit,
                // Ensure creation date is in UTC
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _dataService.CreateTestAsync(newTest);
            Close();
        }
    }
}