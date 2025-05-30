using Avalonia.Controls;
using AvaloniaAzora.Services;
using AvaloniaAzora.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace AvaloniaAzora.Views.Teacher
{
    public partial class AssignTestToClassWindow : Window
    {
        private readonly Guid _classId;
        private readonly Guid _teacherId;
        private readonly Guid? _preSelectedTestId;
        private readonly IDataService _dataService;
        private readonly AssignTestToClassViewModel _viewModel;
        private AvaloniaAzora.Models.Test? _selectedTest;

        // This constructor is for design-time only
        public AssignTestToClassWindow()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            _viewModel = new AssignTestToClassViewModel(_dataService);
            InitializeComponent();
        }

        public AssignTestToClassWindow(Guid classId, Guid teacherId)
        {
            _classId = classId;
            _teacherId = teacherId;
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();

            // Initialize the ViewModel first
            _viewModel = new AssignTestToClassViewModel(_dataService);

            InitializeComponent();

            // Set initial values for controls
            LimitAttemptsNumericUpDown.Value = 1;
            PassingScoreNumericUpDown.Value = 70;

            // Initialize date pickers with default values using local timezone
            var today = DateTimeOffset.Now.Date;
            StartDatePicker.SelectedDate = today;
            DueDatePicker.SelectedDate = today.AddDays(7);

            // Load data when window opens
            this.Opened += async (s, e) => await LoadData();
        }

        public AssignTestToClassWindow(Guid classId, Guid teacherId, Guid preSelectedTestId) : this(classId, teacherId)
        {
            _preSelectedTestId = preSelectedTestId;
        }

        private async Task LoadData()
        {
            await _viewModel.LoadDataAsync(_classId, _teacherId);

            // Update UI manually
            ClassNameText.Text = $"Class: {_viewModel.ClassName}";

            // Create test items
            CreateTestItems();

            // Pre-select test if specified
            if (_preSelectedTestId.HasValue)
            {
                var testToSelect = _viewModel.AvailableTests.FirstOrDefault(t => t.Id == _preSelectedTestId.Value);
                if (testToSelect != null)
                {
                    SelectTest(testToSelect);
                }
            }
        }

        private void CreateTestItems()
        {
            TestsListPanel.Children.Clear();

            if (_viewModel.AvailableTests.Count == 0)
            {
                var emptyState = new Border
                {
                    Background = Avalonia.Media.Brushes.White,
                    CornerRadius = new Avalonia.CornerRadius(8),
                    Padding = new Avalonia.Thickness(24),
                    Margin = new Avalonia.Thickness(0, 12),
                    BorderBrush = Avalonia.Media.Brushes.LightGray,
                    BorderThickness = new Avalonia.Thickness(1)
                };

                var emptyPanel = new StackPanel
                {
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };

                emptyPanel.Children.Add(new TextBlock
                {
                    Text = "📝",
                    FontSize = 32,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Margin = new Avalonia.Thickness(0, 0, 0, 8)
                });

                emptyPanel.Children.Add(new TextBlock
                {
                    Text = "No tests available",
                    FontSize = 16,
                    FontWeight = Avalonia.Media.FontWeight.Medium,
                    Foreground = Avalonia.Media.Brushes.Gray,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Margin = new Avalonia.Thickness(0, 0, 0, 4)
                });

                emptyPanel.Children.Add(new TextBlock
                {
                    Text = "Create a test first to assign it to this class",
                    FontSize = 14,
                    Foreground = Avalonia.Media.Brushes.LightSlateGray,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                });

                emptyState.Child = emptyPanel;
                TestsListPanel.Children.Add(emptyState);
                return;
            }

            foreach (var test in _viewModel.AvailableTests)
            {
                var testItem = CreateTestItem(test);
                TestsListPanel.Children.Add(testItem);
            }
        }

        private Border CreateTestItem(AvaloniaAzora.Models.Test test)
        {
            var testItem = new Border
            {
                Classes = { "test-item" },
                Tag = test,
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
            };

            var mainGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto")
            };

            // Test content
            var contentPanel = new StackPanel();

            var titleText = new TextBlock
            {
                Text = test.Title,
                FontSize = 16,
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                Foreground = Avalonia.Media.Brushes.Black,
                Margin = new Avalonia.Thickness(0, 0, 0, 4)
            };

            var descriptionText = new TextBlock
            {
                Text = test.Description ?? "No description",
                FontSize = 14,
                Foreground = Avalonia.Media.Brushes.Gray,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(0, 0, 0, 8)
            };

            var infoPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 16
            };

            infoPanel.Children.Add(new TextBlock
            {
                Text = $"⏱ {test.TimeLimit} min",
                FontSize = 12,
                Foreground = Avalonia.Media.Brushes.LightSlateGray
            });

            infoPanel.Children.Add(new TextBlock
            {
                Text = $"📝 {test.Questions?.Count ?? 0} questions",
                FontSize = 12,
                Foreground = Avalonia.Media.Brushes.LightSlateGray
            });

            infoPanel.Children.Add(new TextBlock
            {
                Text = $"📅 {test.CreatedAt:MMM dd, yyyy}",
                FontSize = 12,
                Foreground = Avalonia.Media.Brushes.LightSlateGray
            });

            contentPanel.Children.Add(titleText);
            contentPanel.Children.Add(descriptionText);
            contentPanel.Children.Add(infoPanel);

            // Action buttons
            var actionsPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Spacing = 8
            };

            var editButton = new Button
            {
                Content = "✏️ Edit",
                Classes = { "secondary" },
                Tag = test,
                MinWidth = 70,
                FontSize = 12,
                Padding = new Avalonia.Thickness(12, 6)
            };
            editButton.Click += OnEditTestClicked;

            var selectButton = new Button
            {
                Content = "Select",
                Classes = { "action" },
                Tag = test,
                MinWidth = 70,
                FontSize = 12,
                Padding = new Avalonia.Thickness(12, 6)
            };
            selectButton.Click += OnSelectTestClicked;

            actionsPanel.Children.Add(editButton);
            actionsPanel.Children.Add(selectButton);

            Grid.SetColumn(contentPanel, 0);
            Grid.SetColumn(actionsPanel, 1);

            mainGrid.Children.Add(contentPanel);
            mainGrid.Children.Add(actionsPanel);

            testItem.Child = mainGrid;

            // Click handler for the entire item
            testItem.PointerPressed += (s, e) =>
            {
                if (e.GetCurrentPoint(testItem).Properties.IsLeftButtonPressed)
                {
                    SelectTest(test);
                }
            };

            return testItem;
        }

        private void SelectTest(AvaloniaAzora.Models.Test test)
        {
            _selectedTest = test;
            _viewModel.SelectedTest = test;

            // Update selected test info
            SelectedTestTitle.Text = test.Title;
            SelectedTestDescription.Text = test.Description ?? "No description available";

            // Show assignment panel and hide empty state
            AssignmentPanel.IsVisible = true;
            EmptyStatePanel.IsVisible = false;
            ActionButtonsPanel.IsVisible = true;

            // Update visual selection
            UpdateTestItemSelection();
        }

        private void UpdateTestItemSelection()
        {
            foreach (var child in TestsListPanel.Children.OfType<Border>())
            {
                if (child.Tag is AvaloniaAzora.Models.Test test)
                {
                    if (_selectedTest != null && test.Id == _selectedTest.Id)
                    {
                        child.Classes.Add("selected");
                    }
                    else
                    {
                        child.Classes.Remove("selected");
                    }
                }
            }
        }

        private void OnSelectTestClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is AvaloniaAzora.Models.Test test)
            {
                SelectTest(test);
            }
        }

        private void OnEditTestClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is AvaloniaAzora.Models.Test test)
            {
                // Create a dummy ClassTest for editing
                var classTest = new AvaloniaAzora.Models.ClassTest
                {
                    TestId = test.Id,
                    Test = test
                };

                var editWindow = new EditTestWindow(classTest);
                editWindow.Closed += async (s, args) => await LoadData(); // Refresh when closed
                editWindow.Show();
            }
        }

        private void OnCreateNewTestClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var createTestWindow = new CreateTestWindow(_teacherId);
            createTestWindow.Closed += async (s, args) => await LoadData(); // Refresh when closed
            createTestWindow.Show();
        }

        private void OnCancelClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        private async void OnAssignClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!ValidateAssignment())
                return;

            try
            {
                // Update viewmodel with UI values, ensuring proper timezone handling
                _viewModel.SelectedTest = _selectedTest;

                // Convert local date to proper DateTimeOffset with local timezone
                var startDate = StartDatePicker.SelectedDate?.Date ?? DateTimeOffset.Now.Date;
                var dueDate = DueDatePicker.SelectedDate?.Date ?? DateTimeOffset.Now.Date.AddDays(7);

                // Create DateTimeOffset with local timezone for the selected dates
                _viewModel.StartDate = new DateTimeOffset(startDate.Date, DateTimeOffset.Now.Offset);
                _viewModel.DueDate = new DateTimeOffset(dueDate.Date, DateTimeOffset.Now.Offset);

                _viewModel.LimitAttempts = (int)(LimitAttemptsNumericUpDown.Value ?? 1);
                _viewModel.PassingScore = (double)(PassingScoreNumericUpDown.Value ?? 70);

                await _viewModel.ExecuteAssignTest();

                Console.WriteLine("✅ Test assigned successfully!");
                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error assigning test: {ex.Message}");
                ShowAssignmentError($"Failed to assign test: {ex.Message}");
            }
        }

        private bool ValidateAssignment()
        {
            // Validate that a test is selected
            if (_selectedTest == null)
            {
                ShowAssignmentError("Please select a test to assign.");
                return false;
            }

            // Validate dates
            var startDate = StartDatePicker.SelectedDate?.Date;
            var dueDate = DueDatePicker.SelectedDate?.Date;

            if (startDate == null)
            {
                ShowAssignmentError("Please select a start date.");
                StartDatePicker.Focus();
                return false;
            }

            if (dueDate == null)
            {
                ShowAssignmentError("Please select a due date.");
                DueDatePicker.Focus();
                return false;
            }

            // Validate that due date is after start date
            if (dueDate <= startDate)
            {
                ShowAssignmentError("Due date must be after the start date.");
                DueDatePicker.Focus();
                return false;
            }

            // Validate that start date is not in the past (more than 1 day ago)
            if (startDate < DateTimeOffset.Now.Date.AddDays(-1))
            {
                ShowAssignmentError("Start date cannot be more than 1 day in the past.");
                StartDatePicker.Focus();
                return false;
            }

            // Validate attempt limit
            var attemptLimit = LimitAttemptsNumericUpDown.Value ?? 1;
            if (attemptLimit < 1)
            {
                ShowAssignmentError("Attempt limit must be at least 1.");
                LimitAttemptsNumericUpDown.Focus();
                return false;
            }

            // Validate passing score
            var passingScore = PassingScoreNumericUpDown.Value ?? 70;
            if (passingScore < 0 || passingScore > 100)
            {
                ShowAssignmentError("Passing score must be between 0 and 100.");
                PassingScoreNumericUpDown.Focus();
                return false;
            }

            return true;
        }

        private void ShowAssignmentError(string message)
        {
            // For now, show a simple console message
            // In a real application, you would show a proper error message UI
            Console.WriteLine($"❌ Assignment Error: {message}");

            // TODO: Implement proper error message display similar to CreateClassroomWindow
            // This could be a toast notification, status bar message, or inline error display
        }
    }
}