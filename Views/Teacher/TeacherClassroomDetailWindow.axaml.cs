using Avalonia.Controls;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using AvaloniaAzora.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaAzora.Views.Teacher
{
    public partial class TeacherClassroomDetailWindow : Window
    {
        private readonly Guid _classId;
        private readonly Guid _teacherId;
        private readonly IDataService _dataService;
        private readonly TeacherClassroomDetailViewModel _viewModel;
        private readonly ObservableCollection<User> _studentsCollection;

        // This constructor is for design-time only
        public TeacherClassroomDetailWindow()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            _viewModel = new TeacherClassroomDetailViewModel(_dataService);
            _studentsCollection = new ObservableCollection<User>();
            InitializeComponent();
        }

        public TeacherClassroomDetailWindow(Guid classId, Guid teacherId)
        {
            _classId = classId;
            _teacherId = teacherId;
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();

            // Initialize the ViewModel first
            _viewModel = new TeacherClassroomDetailViewModel(_dataService);
            _studentsCollection = new ObservableCollection<User>();

            InitializeComponent();

            // Load data when window opens
            this.Opened += async (s, e) => await LoadData();
        }

        private async Task LoadData()
        {
            await _viewModel.LoadClassroomDataAsync(_classId);

            // Update header info
            ClassNameText.Text = _viewModel.ClassName;
            DescriptionText.Text = _viewModel.Description ?? "No description available";

            // Update info cards
            StudentCountText.Text = _viewModel.Students.Count.ToString();
            TestCountText.Text = _viewModel.Tests.Count.ToString();

            // Update tab counts
            StudentsTabCountText.Text = $"Students ({_viewModel.Students.Count})";
            TestsTabCountText.Text = $"Tests ({_viewModel.Tests.Count})";

            // Create student and test cards
            CreateStudentCards();
            await CreateTestCards();
        }

        private void CreateStudentCards()
        {
            StudentsPanel.Children.Clear();

            if (_viewModel.Students.Count == 0)
            {
                var emptyState = new Border
                {
                    Background = Avalonia.Media.Brushes.White,
                    CornerRadius = new Avalonia.CornerRadius(8),
                    Padding = new Avalonia.Thickness(40),
                    Margin = new Avalonia.Thickness(0, 20),
                    BorderBrush = Avalonia.Media.Brushes.LightGray,
                    BorderThickness = new Avalonia.Thickness(1, 1, 1, 1)
                };

                var emptyPanel = new StackPanel
                {
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };

                emptyPanel.Children.Add(new TextBlock
                {
                    Text = "👥",
                    FontSize = 48,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Margin = new Avalonia.Thickness(0, 0, 0, 16)
                });

                emptyPanel.Children.Add(new TextBlock
                {
                    Text = "No students enrolled",
                    FontSize = 18,
                    FontWeight = Avalonia.Media.FontWeight.Medium,
                    Foreground = Avalonia.Media.Brushes.Gray,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Margin = new Avalonia.Thickness(0, 0, 0, 8)
                });

                emptyPanel.Children.Add(new TextBlock
                {
                    Text = "Add students to get started with this class",
                    FontSize = 14,
                    Foreground = Avalonia.Media.Brushes.LightSlateGray,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                });

                emptyState.Child = emptyPanel;
                StudentsPanel.Children.Add(emptyState);
                return;
            }

            foreach (var student in _viewModel.Students)
            {
                var card = CreateStudentCard(student);
                StudentsPanel.Children.Add(card);
            }
        }

        private Border CreateStudentCard(User student)
        {
            var card = new Border
            {
                Classes = { "student-card" }
            };

            var mainGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto")
            };

            // Avatar
            var avatar = new Border
            {
                Background = Avalonia.Media.Brushes.LightBlue,
                CornerRadius = new Avalonia.CornerRadius(25),
                Width = 50,
                Height = 50,
                Margin = new Avalonia.Thickness(0, 0, 16, 0),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            avatar.Child = new TextBlock
            {
                Text = student.FullName?.Substring(0, 1).ToUpper() ?? "S",
                FontSize = 20,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Foreground = Avalonia.Media.Brushes.White,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

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
                Foreground = Avalonia.Media.Brushes.Black,
                Margin = new Avalonia.Thickness(0, 0, 0, 4)
            };

            var emailText = new TextBlock
            {
                Text = student.Email ?? "No email",
                FontSize = 14,
                Foreground = Avalonia.Media.Brushes.Gray
            };

            infoPanel.Children.Add(nameText);
            infoPanel.Children.Add(emailText);

            // Actions Panel with Status Badge and Remove Button
            var actionsPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Spacing = 8
            };

            var statusBadge = new Border
            {
                Classes = { "status-badge", "status-active" },
                CornerRadius = new Avalonia.CornerRadius(12),
                Padding = new Avalonia.Thickness(8, 4)
            };
            statusBadge.Child = new TextBlock
            {
                Text = "Active",
                FontSize = 12,
                FontWeight = Avalonia.Media.FontWeight.Medium,
                Foreground = Avalonia.Media.Brushes.White
            };

            var removeButton = new Button
            {
                Content = "Remove",
                Classes = { "secondary" },
                Tag = student,
                FontSize = 12,
                Padding = new Avalonia.Thickness(8, 4)
            };
            removeButton.Click += OnRemoveStudentClicked;

            actionsPanel.Children.Add(statusBadge);
            actionsPanel.Children.Add(removeButton);

            Grid.SetColumn(avatar, 0);
            Grid.SetColumn(infoPanel, 1);
            Grid.SetColumn(actionsPanel, 2);

            mainGrid.Children.Add(avatar);
            mainGrid.Children.Add(infoPanel);
            mainGrid.Children.Add(actionsPanel);

            card.Child = mainGrid;
            return card;
        }

        private async Task CreateTestCards()
        {
            TestsPanel.Children.Clear();

            if (!_viewModel.Tests.Any())
            {
                // Show empty state
                var emptyState = new Border
                {
                    Background = Avalonia.Media.Brushes.Transparent,
                    CornerRadius = new Avalonia.CornerRadius(12),
                    Padding = new Avalonia.Thickness(32),
                    Margin = new Avalonia.Thickness(0, 20, 0, 0)
                };

                var emptyPanel = new StackPanel
                {
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                };

                emptyPanel.Children.Add(new Border
                {
                    Background = Avalonia.Media.Brushes.LightGray,
                    CornerRadius = new Avalonia.CornerRadius(50),
                    Width = 80,
                    Height = 80,
                    Margin = new Avalonia.Thickness(0, 0, 0, 16),
                    Child = new TextBlock
                    {
                        Text = "📝",
                        FontSize = 32,
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    }
                });

                emptyPanel.Children.Add(new TextBlock
                {
                    Text = "No Tests Yet",
                    FontSize = 18,
                    FontWeight = Avalonia.Media.FontWeight.SemiBold,
                    Foreground = Avalonia.Media.Brushes.Black,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
                });

                emptyPanel.Children.Add(new TextBlock
                {
                    Text = "Create or assign tests to get started",
                    FontSize = 14,
                    Foreground = Avalonia.Media.Brushes.Gray,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Margin = new Avalonia.Thickness(0, 8, 0, 0)
                });

                emptyState.Child = emptyPanel;
                TestsPanel.Children.Add(emptyState);
                return;
            }

            foreach (var classTest in _viewModel.Tests)
            {
                var card = await CreateTestCardAsync(classTest);
                TestsPanel.Children.Add(card);
            }
        }

        private async Task<Border> CreateTestCardAsync(ClassTest classTest)
        {
            var card = new Border
            {
                Classes = { "test-card" },
                Tag = classTest,
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
            };

            // Add click handler to the card to navigate to test analytics
            card.PointerPressed += (s, e) =>
            {
                if (e.GetCurrentPoint(card).Properties.IsLeftButtonPressed)
                {
                    OpenTestAnalytics(classTest);
                }
            };

            var mainGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto")
            };

            // Test Content
            var contentPanel = new StackPanel();

            // Test Header
            var headerGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                Margin = new Avalonia.Thickness(0, 0, 0, 12)
            };

            var titleText = new TextBlock
            {
                Text = classTest.Test?.Title ?? "Unknown Test",
                FontSize = 18,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Foreground = Avalonia.Media.Brushes.Black
            };

            var statusBadge = new Border
            {
                CornerRadius = new Avalonia.CornerRadius(12),
                Padding = new Avalonia.Thickness(8, 4),
                Background = GetTestStatusColor(classTest),
                Margin = new Avalonia.Thickness(0, 0, 16, 0) // Add spacing from action buttons
            };
            statusBadge.Child = new TextBlock
            {
                Text = GetTestStatusText(classTest),
                FontSize = 12,
                FontWeight = Avalonia.Media.FontWeight.Medium,
                Foreground = Avalonia.Media.Brushes.White
            };

            Grid.SetColumn(titleText, 0);
            Grid.SetColumn(statusBadge, 1);
            headerGrid.Children.Add(titleText);
            headerGrid.Children.Add(statusBadge);

            // Test Details
            var detailsPanel = new StackPanel
            {
                Margin = new Avalonia.Thickness(0, 0, 0, 12)
            };

            if (!string.IsNullOrEmpty(classTest.Test?.Description))
            {
                var descText = new TextBlock
                {
                    Text = classTest.Test.Description,
                    FontSize = 14,
                    Foreground = Avalonia.Media.Brushes.Gray,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(0, 0, 0, 8)
                };
                detailsPanel.Children.Add(descText);
            }

            // Test Info Grid
            var infoGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,*,*")
            };

            // Get actual question count from database
            int questionCount = 0;
            if (classTest.Test?.Id != null)
            {
                var questions = await _dataService.GetQuestionsByTestIdAsync(classTest.Test.Id);
                questionCount = questions.Count;
            }

            var questionsInfo = new StackPanel();
            questionsInfo.Children.Add(new TextBlock
            {
                Text = questionCount.ToString(),
                FontSize = 16,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Foreground = Avalonia.Media.Brushes.Black
            });
            questionsInfo.Children.Add(new TextBlock
            {
                Text = "Questions",
                FontSize = 12,
                Foreground = Avalonia.Media.Brushes.Gray
            });

            var timeLimitInfo = new StackPanel();
            timeLimitInfo.Children.Add(new TextBlock
            {
                Text = $"{classTest.Test?.TimeLimit ?? 0}m",
                FontSize = 16,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Foreground = Avalonia.Media.Brushes.Black
            });
            timeLimitInfo.Children.Add(new TextBlock
            {
                Text = "Time Limit",
                FontSize = 12,
                Foreground = Avalonia.Media.Brushes.Gray
            });

            var dueDateInfo = new StackPanel();
            dueDateInfo.Children.Add(new TextBlock
            {
                Text = classTest.DueDate?.ToString("MMM dd") ?? "No due date",
                FontSize = 16,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Foreground = Avalonia.Media.Brushes.Black
            });
            dueDateInfo.Children.Add(new TextBlock
            {
                Text = "Due Date",
                FontSize = 12,
                Foreground = Avalonia.Media.Brushes.Gray
            });

            Grid.SetColumn(questionsInfo, 0);
            Grid.SetColumn(timeLimitInfo, 1);
            Grid.SetColumn(dueDateInfo, 2);
            infoGrid.Children.Add(questionsInfo);
            infoGrid.Children.Add(timeLimitInfo);
            infoGrid.Children.Add(dueDateInfo);

            detailsPanel.Children.Add(infoGrid);

            contentPanel.Children.Add(headerGrid);
            contentPanel.Children.Add(detailsPanel);

            // Action Buttons
            var actionsPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Spacing = 8
            };

            var editButton = new Button
            {
                Content = "Edit",
                Classes = { "secondary" },
                Tag = classTest,
                MinWidth = 80
            };
            editButton.Click += OnEditTestClicked;

            var removeButton = new Button
            {
                Content = "Remove",
                Classes = { "secondary" },
                Tag = classTest,
                MinWidth = 80
            };
            removeButton.Click += OnRemoveTestClicked;

            actionsPanel.Children.Add(editButton);
            actionsPanel.Children.Add(removeButton);

            Grid.SetColumn(contentPanel, 0);
            Grid.SetColumn(actionsPanel, 1);

            mainGrid.Children.Add(contentPanel);
            mainGrid.Children.Add(actionsPanel);

            card.Child = mainGrid;
            return card;
        }

        private Avalonia.Media.IBrush GetTestStatusColor(ClassTest classTest)
        {
            if (classTest.DueDate.HasValue)
            {
                if (classTest.DueDate.Value < DateTimeOffset.UtcNow)
                    return Avalonia.Media.Brushes.Gray; // Past due
                else if (classTest.DueDate.Value <= DateTimeOffset.UtcNow.AddDays(3))
                    return Avalonia.Media.Brushes.Orange; // Due soon
                else
                    return Avalonia.Media.Brushes.Green; // Active
            }
            return Avalonia.Media.Brushes.Blue; // No due date
        }

        private string GetTestStatusText(ClassTest classTest)
        {
            if (classTest.DueDate.HasValue)
            {
                if (classTest.DueDate.Value < DateTimeOffset.UtcNow)
                    return "Past Due";
                else if (classTest.DueDate.Value <= DateTimeOffset.UtcNow.AddDays(3))
                    return "Due Soon";
                else
                    return "Active";
            }
            return "No Due Date";
        }

        private void OnBackClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        private void OnAddStudentClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var addStudentWindow = new AddStudentToClassWindow(_classId);
            addStudentWindow.Closed += async (s, args) => await LoadData();
            addStudentWindow.Show();
        }

        private void OnCreateTestClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Just open the create test window, don't chain to assign test
            var createTestWindow = new CreateTestWindow(_teacherId);
            createTestWindow.Closed += async (s, args) => await LoadData(); // Refresh data when closed
            createTestWindow.Show();
        }

        private void OnAddTestClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var assignWindow = new AssignTestToClassWindow(_classId, _teacherId);
            assignWindow.Closed += async (s, args) => await LoadData();
            assignWindow.Show();
        }

        private void OnEditTestClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ClassTest classTest)
            {
                var editWindow = new EditTestWindow(classTest);
                editWindow.Closed += async (s, args) => await LoadData();
                editWindow.Show();
            }
        }

        private async void OnRemoveTestClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ClassTest classTest)
            {
                // TODO: Add confirmation dialog
                await _viewModel.RemoveTestCommand.ExecuteAsync(classTest.Id);
                await LoadData();
            }
        }

        private async void OnRemoveStudentClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is User student)
            {
                // Find the enrollment to remove
                var enrollment = await _dataService.GetEnrollmentsByClassIdAsync(_classId);
                var studentEnrollment = enrollment.FirstOrDefault(en => en.StudentId == student.Id);

                if (studentEnrollment != null)
                {
                    // Remove the enrollment
                    await _dataService.RemoveEnrollmentAsync(studentEnrollment.Id);

                    // Refresh data
                    await LoadData();
                }
            }
        }

        private void OpenTestAnalytics(ClassTest classTest)
        {
            var analyticsWindow = new TestAnalyticsWindow(classTest);
            analyticsWindow.Show();
        }
    }
}