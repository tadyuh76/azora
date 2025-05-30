using Avalonia.Controls;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using AvaloniaAzora.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

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
            var className = _viewModel.ClassName;
            if (string.IsNullOrWhiteSpace(className))
            {
                // TODO: show validation message
                return;
            }
            var newClass = new Class
            {
                TeacherId = _teacherId,
                ClassName = className,
                Description = _viewModel.Description,
                // Ensure creation date is in UTC
                CreatedAt = DateTimeOffset.UtcNow
            };

            try
            {
                var createdClass = await _dataService.CreateClassAsync(newClass);
                WasClassroomCreated = true;

                // Raise the ClassroomCreated event
                ClassroomCreated?.Invoke(this, new ClassroomCreatedEventArgs { ClassroomId = createdClass.Id });

                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating classroom: {ex.Message}");
                // TODO: Show error message to user
            }
        }
    }

    public class ClassroomCreatedEventArgs : EventArgs
    {
        public Guid ClassroomId { get; set; }
    }
}