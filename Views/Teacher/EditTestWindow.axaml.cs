using Avalonia.Controls;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using AvaloniaAzora.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AvaloniaAzora.Views.Teacher
{
    public partial class EditTestWindow : Window
    {
        private readonly ClassTest? _classTest;
        private readonly IDataService _dataService;
        private readonly EditTestViewModel _viewModel;

        // This constructor is for design-time only
        public EditTestWindow()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            _viewModel = new EditTestViewModel(_dataService);
            InitializeComponent();
        }

        public EditTestWindow(ClassTest classTest)
        {
            _classTest = classTest;
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();

            // Initialize the ViewModel first
            _viewModel = new EditTestViewModel(_dataService);

            InitializeComponent();

            // Set default values
            QuestionTypeComboBox.SelectedIndex = 0; // Default to multiple choice
            QuestionPointsNumericUpDown.Value = 1;

            // Set up empty questions panel visibility
            UpdateQuestionsUI();

            // Set up question type change handler
            OnQuestionTypeChanged(null, null);

            // Load data when window opens
            this.Opened += (s, e) => LoadData();
        }

        private void OnQuestionTypeChanged(object? sender, SelectionChangedEventArgs? e)
        {
            if (QuestionTypeComboBox?.SelectedItem is ComboBoxItem selectedItem)
            {
                var questionType = selectedItem.Tag?.ToString();

                if (questionType == "multiple_choice")
                {
                    // Show multiple choice fields
                    AnswerOptionsPanel.IsVisible = true;
                    MultipleChoiceAnswerPanel.IsVisible = true;
                    ShortAnswerPanel.IsVisible = false;
                }
                else if (questionType == "short_answer")
                {
                    // Show short answer fields
                    AnswerOptionsPanel.IsVisible = false;
                    MultipleChoiceAnswerPanel.IsVisible = false;
                    ShortAnswerPanel.IsVisible = true;
                }
            }
        }

        private async void LoadData()
        {
            // Fix nullable Guid issue
            if (_classTest?.TestId.HasValue == true)
            {
                await _viewModel.LoadTestDataAsync(_classTest.TestId.Value);
            }

            // Update UI with test details
            TitleTextBox.Text = _viewModel.Title;
            DescriptionTextBox.Text = _viewModel.Description;
            TimeLimitNumericUpDown.Value = _viewModel.TimeLimit;

            // Update questions display
            CreateQuestionCards();
            UpdateQuestionsUI();
        }

        private void CreateQuestionCards()
        {
            QuestionsPanel.Children.Clear();

            foreach (var question in _viewModel.Questions)
            {
                var card = CreateQuestionCard(question);
                QuestionsPanel.Children.Add(card);
            }
        }

        private Border CreateQuestionCard(Question question)
        {
            var card = new Border
            {
                Classes = { "question-card" }
            };

            var mainGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto")
            };

            var contentPanel = new StackPanel();

            // Question header
            var headerGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                Margin = new Avalonia.Thickness(0, 0, 0, 12)
            };

            var questionText = new TextBlock
            {
                Text = question.Text,
                FontWeight = Avalonia.Media.FontWeight.SemiBold,
                FontSize = 16,
                Foreground = Avalonia.Media.Brushes.Black,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };

            var pointsBadge = new Border
            {
                Background = Avalonia.Media.Brushes.LightBlue,
                CornerRadius = new Avalonia.CornerRadius(12),
                Padding = new Avalonia.Thickness(8, 4)
            };
            pointsBadge.Child = new TextBlock
            {
                Text = $"{question.Points} pts",
                FontSize = 12,
                FontWeight = Avalonia.Media.FontWeight.Medium,
                Foreground = Avalonia.Media.Brushes.DarkBlue
            };

            Grid.SetColumn(questionText, 0);
            Grid.SetColumn(pointsBadge, 1);
            headerGrid.Children.Add(questionText);
            headerGrid.Children.Add(pointsBadge);

            // Question details
            var detailsPanel = new StackPanel
            {
                Margin = new Avalonia.Thickness(0, 0, 0, 12)
            };

            var typeText = new TextBlock
            {
                Text = $"Type: {GetDisplayQuestionType(question.Type ?? "unknown")}",
                FontSize = 14,
                Foreground = Avalonia.Media.Brushes.Gray,
                Margin = new Avalonia.Thickness(0, 0, 0, 6)
            };

            var correctAnswerText = new TextBlock
            {
                Text = GetCorrectAnswerDisplay(question),
                FontSize = 14,
                Foreground = Avalonia.Media.Brushes.Green,
                FontWeight = Avalonia.Media.FontWeight.Medium
            };

            detailsPanel.Children.Add(typeText);
            detailsPanel.Children.Add(correctAnswerText);

            // Possible answers (if available)
            if (question.Answers?.Any() == true)
            {
                var answersText = new TextBlock
                {
                    Text = $"Options: {string.Join(", ", question.Answers.Take(4))}",
                    FontSize = 14,
                    Foreground = Avalonia.Media.Brushes.Gray,
                    Margin = new Avalonia.Thickness(0, 6, 0, 0),
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                };
                detailsPanel.Children.Add(answersText);
            }

            contentPanel.Children.Add(headerGrid);
            contentPanel.Children.Add(detailsPanel);

            // Delete button
            var deleteButton = new Button
            {
                Content = "Delete",
                Classes = { "secondary" },
                Tag = question,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                MinWidth = 80
            };
            deleteButton.Click += OnDeleteQuestionClicked;

            Grid.SetColumn(contentPanel, 0);
            Grid.SetColumn(deleteButton, 1);

            mainGrid.Children.Add(contentPanel);
            mainGrid.Children.Add(deleteButton);

            card.Child = mainGrid;

            return card;
        }

        private string GetDisplayQuestionType(string type)
        {
            return type switch
            {
                "multiple_choice" => "Multiple Choice",
                "short_answer" => "Short Answer",
                _ => type
            };
        }

        private string GetCorrectAnswerDisplay(Question question)
        {
            if (question.Type == "multiple_choice" && question.Answers?.Any() == true)
            {
                // For multiple choice, CorrectAnswer contains the index (1-4)
                if (int.TryParse(question.CorrectAnswer, out int index) && index >= 1 && index <= 4)
                {
                    var answerIndex = index - 1; // Convert to 0-based index
                    if (answerIndex < question.Answers.Count())
                    {
                        var optionLetter = (char)('A' + answerIndex);
                        return $"Correct Answer: {optionLetter}. {question.Answers.ElementAt(answerIndex)}";
                    }
                }
            }
            else if (question.Type == "short_answer")
            {
                return $"Correct Answer: {question.CorrectAnswer}";
            }
            return "Correct Answer: N/A";
        }

        private void UpdateQuestionsUI()
        {
            // Update questions count
            QuestionsCountText.Text = $"Questions ({_viewModel.Questions.Count})";

            // Update empty state visibility
            EmptyQuestionsPanel.IsVisible = _viewModel.Questions.Count == 0;
        }

        private async void OnUpdateTestClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!ValidateTestInput())
                return;

            try
            {
                // Update viewmodel with UI values
                _viewModel.Title = TitleTextBox.Text ?? string.Empty;
                _viewModel.Description = DescriptionTextBox.Text ?? string.Empty;
                _viewModel.TimeLimit = (int)(TimeLimitNumericUpDown.Value ?? 60);

                await _viewModel.UpdateTestCommand.ExecuteAsync(null);

                Console.WriteLine("✅ Test updated successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error updating test: {ex.Message}");
                ShowValidationError($"Failed to update test: {ex.Message}");
            }
        }

        private bool ValidateTestInput()
        {
            // Validate test title
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                ShowValidationError("Please enter a test title.");
                TitleTextBox.Focus();
                return false;
            }

            if (TitleTextBox.Text.Trim().Length < 3)
            {
                ShowValidationError("Test title must be at least 3 characters long.");
                TitleTextBox.Focus();
                return false;
            }

            if (TitleTextBox.Text.Trim().Length > 100)
            {
                ShowValidationError("Test title cannot exceed 100 characters.");
                TitleTextBox.Focus();
                return false;
            }

            // Validate time limit
            var timeLimit = TimeLimitNumericUpDown.Value ?? 0;
            if (timeLimit < 1)
            {
                ShowValidationError("Time limit must be at least 1 minute.");
                TimeLimitNumericUpDown.Focus();
                return false;
            }

            if (timeLimit > 480)
            {
                ShowValidationError("Time limit cannot exceed 480 minutes (8 hours).");
                TimeLimitNumericUpDown.Focus();
                return false;
            }

            return true;
        }

        private async void OnAddQuestionClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!ValidateQuestionInput())
                return;

            try
            {
                // Get question type from ComboBox
                var selectedTypeItem = QuestionTypeComboBox.SelectedItem as ComboBoxItem;
                var questionType = selectedTypeItem?.Tag?.ToString() ?? "multiple_choice";

                // Update viewmodel with UI values
                _viewModel.NewQuestionText = NewQuestionTextBox.Text ?? string.Empty;
                _viewModel.NewQuestionType = questionType;
                _viewModel.QuestionPoints = (int)(QuestionPointsNumericUpDown.Value ?? 1);

                // Handle correct answer based on question type
                if (questionType == "multiple_choice")
                {
                    var selectedAnswerItem = CorrectAnswerComboBox.SelectedItem as ComboBoxItem;
                    var selectedOption = selectedAnswerItem?.Tag?.ToString() ?? "A";

                    // Map option letters to index (1-4) for correct answer storage
                    var answerIndexMap = new Dictionary<string, string>
                    {
                        ["A"] = "1",
                        ["B"] = "2",
                        ["C"] = "3",
                        ["D"] = "4"
                    };

                    // Store the index instead of the text
                    _viewModel.CorrectAnswer = answerIndexMap[selectedOption];

                    // Copy possible answers from UI to viewmodel
                    _viewModel.PossibleAnswers.Clear();
                    var answers = new List<string>
                    {
                        Answer1TextBox.Text ?? string.Empty,
                        Answer2TextBox.Text ?? string.Empty,
                        Answer3TextBox.Text ?? string.Empty,
                        Answer4TextBox.Text ?? string.Empty
                    };

                    foreach (var answer in answers.Where(a => !string.IsNullOrWhiteSpace(a)))
                    {
                        _viewModel.PossibleAnswers.Add(answer);
                    }
                }
                else
                {
                    // Short answer
                    _viewModel.CorrectAnswer = CorrectAnswerTextBox.Text ?? string.Empty;
                    _viewModel.PossibleAnswers.Clear();
                }

                await _viewModel.AddQuestionCommand.ExecuteAsync(null);

                // Clear form
                ClearQuestionForm();

                // Update questions display
                CreateQuestionCards();
                UpdateQuestionsUI();

                // Show success message (you could implement this similar to other forms)
                Console.WriteLine("✅ Question added successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error adding question: {ex.Message}");
                // TODO: Show error message to user
            }
        }

        private bool ValidateQuestionInput()
        {
            // Validate question text
            if (string.IsNullOrWhiteSpace(NewQuestionTextBox.Text))
            {
                ShowValidationError("Please enter a question text.");
                NewQuestionTextBox.Focus();
                return false;
            }

            if (NewQuestionTextBox.Text.Trim().Length < 10)
            {
                ShowValidationError("Question text must be at least 10 characters long.");
                NewQuestionTextBox.Focus();
                return false;
            }

            // Get question type
            var selectedTypeItem = QuestionTypeComboBox.SelectedItem as ComboBoxItem;
            var questionType = selectedTypeItem?.Tag?.ToString() ?? "multiple_choice";

            if (questionType == "multiple_choice")
            {
                // Validate that at least 2 answer options are provided
                var answers = new List<string>
                {
                    Answer1TextBox.Text?.Trim() ?? string.Empty,
                    Answer2TextBox.Text?.Trim() ?? string.Empty,
                    Answer3TextBox.Text?.Trim() ?? string.Empty,
                    Answer4TextBox.Text?.Trim() ?? string.Empty
                };

                var validAnswers = answers.Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
                if (validAnswers.Count < 2)
                {
                    ShowValidationError("Please provide at least 2 answer options for multiple choice questions.");
                    Answer1TextBox.Focus();
                    return false;
                }

                // Validate that a correct answer is selected
                if (CorrectAnswerComboBox.SelectedItem == null)
                {
                    ShowValidationError("Please select the correct answer for the multiple choice question.");
                    CorrectAnswerComboBox.Focus();
                    return false;
                }
            }
            else if (questionType == "short_answer")
            {
                // Validate correct answer for short answer questions
                if (string.IsNullOrWhiteSpace(CorrectAnswerTextBox.Text))
                {
                    ShowValidationError("Please provide the correct answer for the short answer question.");
                    CorrectAnswerTextBox.Focus();
                    return false;
                }

                if (CorrectAnswerTextBox.Text.Trim().Length < 1)
                {
                    ShowValidationError("Correct answer must be at least 1 character long.");
                    CorrectAnswerTextBox.Focus();
                    return false;
                }
            }

            // Validate points
            if (QuestionPointsNumericUpDown.Value < 1)
            {
                ShowValidationError("Question points must be at least 1.");
                QuestionPointsNumericUpDown.Focus();
                return false;
            }

            return true;
        }

        private void ShowValidationError(string message)
        {
            // For now, show a simple console message
            // In a real application, you would show a proper error message UI
            Console.WriteLine($"❌ Validation Error: {message}");

            // TODO: Implement proper error message display similar to CreateClassroomWindow
            // This could be a toast notification, status bar message, or inline error display
        }

        private void ClearQuestionForm()
        {
            NewQuestionTextBox.Text = "";
            CorrectAnswerTextBox.Text = "";
            Answer1TextBox.Text = "";
            Answer2TextBox.Text = "";
            Answer3TextBox.Text = "";
            Answer4TextBox.Text = "";
            CorrectAnswerComboBox.SelectedIndex = 0;
            QuestionPointsNumericUpDown.Value = 1;
        }

        private async void OnDeleteQuestionClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Question question)
            {
                await _viewModel.DeleteQuestionCommand.ExecuteAsync(question);

                // Update questions display
                CreateQuestionCards();
                UpdateQuestionsUI();
            }
        }

        private void OnCloseClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }

        private void OnBackClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close();
        }
    }
}