using Avalonia.Controls;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using AvaloniaAzora.ViewModels.Teacher;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AvaloniaAzora.Views.Teacher
{
    public partial class AddStudentToClassWindow : Window
    {
        private readonly Guid _classId;
        private readonly IDataService _dataService;
        private readonly AddStudentToClassViewModel _viewModel;
        private readonly ObservableCollection<User> _availableStudentsCollection;
        private readonly ObservableCollection<User> _selectedStudentsCollection;

        // This constructor is for design-time only
        public AddStudentToClassWindow()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            _viewModel = new AddStudentToClassViewModel(_dataService);
            _availableStudentsCollection = new ObservableCollection<User>();
            _selectedStudentsCollection = new ObservableCollection<User>();
            InitializeComponent();
        }

        public AddStudentToClassWindow(Guid classId)
        {
            _classId = classId;
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();

            // Initialize the ViewModel first
            _viewModel = new AddStudentToClassViewModel(_dataService);
            _availableStudentsCollection = new ObservableCollection<User>();
            _selectedStudentsCollection = new ObservableCollection<User>();

            InitializeComponent();

            // Set up search functionality
            SearchTextBox.TextChanged += OnSearchTextChanged;

            // Set up collection change handlers
            _selectedStudentsCollection.CollectionChanged += OnSelectedStudentsChanged;

            // Load data when window opens
            this.Opened += (s, e) => LoadData();
        }

        private async void LoadData()
        {
            await _viewModel.LoadDataAsync(_classId);

            // Update UI manually
            ClassNameText.Text = _viewModel.ClassName;

            // Sync collections with viewmodel
            _availableStudentsCollection.Clear();
            foreach (var student in _viewModel.AvailableStudents)
            {
                _availableStudentsCollection.Add(student);
            }

            _selectedStudentsCollection.Clear();
            foreach (var student in _viewModel.SelectedStudents)
            {
                _selectedStudentsCollection.Add(student);
            }

            // Create student cards
            CreateAvailableStudentCards();
            CreateSelectedStudentCards();
            UpdateUI();
        }

        private void CreateAvailableStudentCards()
        {
            AvailableStudentsPanel.Children.Clear();

            foreach (var student in _availableStudentsCollection)
            {
                var card = CreateStudentCard(student, true);
                AvailableStudentsPanel.Children.Add(card);
            }
        }

        private void CreateSelectedStudentCards()
        {
            SelectedStudentsPanel.Children.Clear();

            foreach (var student in _selectedStudentsCollection)
            {
                var card = CreateStudentCard(student, false);
                SelectedStudentsPanel.Children.Add(card);
            }
        }

        private Border CreateStudentCard(User student, bool isAvailable)
        {
            var card = new Border
            {
                Classes = { "student-card" }
            };

            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto")
            };

            // Avatar
            var avatar = new Border
            {
                Background = Avalonia.Media.Brushes.LightBlue,
                CornerRadius = new Avalonia.CornerRadius(20),
                Width = 40,
                Height = 40,
                Margin = new Avalonia.Thickness(0, 0, 12, 0)
            };

            var avatarText = new TextBlock
            {
                Text = GetInitials(student.FullName ?? student.Email),
                FontSize = 16,
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                Foreground = Avalonia.Media.Brushes.White,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            avatar.Child = avatarText;

            // Student Info
            var infoPanel = new StackPanel
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            var nameText = new TextBlock
            {
                Text = student.FullName ?? "Unknown Student",
                FontSize = 16,
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                Foreground = Avalonia.Media.Brushes.Black
            };

            var emailText = new TextBlock
            {
                Text = student.Email,
                FontSize = 14,
                Foreground = Avalonia.Media.Brushes.Gray,
                Margin = new Avalonia.Thickness(0, 2, 0, 0)
            };

            infoPanel.Children.Add(nameText);
            infoPanel.Children.Add(emailText);

            // Action Button
            var actionButton = new Button
            {
                Content = isAvailable ? "Add" : "Remove",
                Classes = { isAvailable ? "action" : "secondary" },
                Padding = new Avalonia.Thickness(16, 8),
                FontSize = 14,
                Tag = student
            };

            if (isAvailable)
            {
                actionButton.Click += OnAddStudentClicked;
            }
            else
            {
                actionButton.Click += OnRemoveStudentClicked;
            }

            Grid.SetColumn(avatar, 0);
            Grid.SetColumn(infoPanel, 1);
            Grid.SetColumn(actionButton, 2);

            grid.Children.Add(avatar);
            grid.Children.Add(infoPanel);
            grid.Children.Add(actionButton);

            card.Child = grid;

            return card;
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "?";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();
            else if (parts.Length == 1)
                return parts[0][0].ToString().ToUpper();
            else
                return "?";
        }

        private void OnAddStudentClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is User student)
            {
                _availableStudentsCollection.Remove(student);
                _selectedStudentsCollection.Add(student);

                // Update viewmodel
                _viewModel.AvailableStudents.Remove(student);
                _viewModel.SelectedStudents.Add(student);

                CreateAvailableStudentCards();
                CreateSelectedStudentCards();
                UpdateUI();
            }
        }

        private void OnRemoveStudentClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is User student)
            {
                _selectedStudentsCollection.Remove(student);
                _availableStudentsCollection.Add(student);

                // Update viewmodel
                _viewModel.SelectedStudents.Remove(student);
                _viewModel.AvailableStudents.Add(student);

                CreateAvailableStudentCards();
                CreateSelectedStudentCards();
                UpdateUI();
            }
        }

        private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text?.ToLower() ?? string.Empty;

            // Filter available students based on search
            AvailableStudentsPanel.Children.Clear();

            foreach (var student in _availableStudentsCollection)
            {
                var studentName = student.FullName?.ToLower() ?? string.Empty;
                var studentEmail = student.Email?.ToLower() ?? string.Empty;

                if (string.IsNullOrEmpty(searchText) ||
                    studentName.Contains(searchText) ||
                    studentEmail.Contains(searchText))
                {
                    var card = CreateStudentCard(student, true);
                    AvailableStudentsPanel.Children.Add(card);
                }
            }
        }

        private void OnSelectedStudentsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            // Update selected count
            var count = _selectedStudentsCollection.Count;
            SelectedCountText.Text = count == 0 ? "No students selected" :
                                   count == 1 ? "1 student selected" :
                                   $"{count} students selected";

            // Update enroll button state
            EnrollButton.IsEnabled = count > 0;

            // Show/hide empty state
            EmptyStatePanel.IsVisible = count == 0;
            SelectedStudentsScrollViewer.IsVisible = count > 0;
        }

        private void OnCancelClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        private async void OnEnrollClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_selectedStudentsCollection.Count == 0)
                return;

            await _viewModel.EnrollStudentsCommand.ExecuteAsync(_classId);
            Close();
        }
    }
}