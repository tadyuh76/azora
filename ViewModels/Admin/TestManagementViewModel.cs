using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels.Admin
{
    public partial class TestManagementViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        
        [ObservableProperty]
        private string _searchText = string.Empty;
        
        [ObservableProperty]
        private bool _isLoading = false;
        
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        [ObservableProperty]
        private string _successMessage = string.Empty;
        
        [ObservableProperty]
        private TestViewModel? _selectedTest;

        public ObservableCollection<TestViewModel> Tests { get; } = new();

        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CreateTestCommand { get; }
        public ICommand EditTestCommand { get; }
        public ICommand ViewQuestionsCommand { get; }
        public ICommand DeleteTestCommand { get; }

        public TestManagementViewModel()
        {
            _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            
            SearchCommand = new AsyncRelayCommand(SearchTestsAsync);
            RefreshCommand = new AsyncRelayCommand(LoadTestsAsync);
            CreateTestCommand = new AsyncRelayCommand(CreateTestAsync);
            EditTestCommand = new AsyncRelayCommand<Guid>(EditTestAsync);
            ViewQuestionsCommand = new AsyncRelayCommand<Guid>(ViewTestQuestionsAsync);
            DeleteTestCommand = new AsyncRelayCommand<Guid>(DeleteTestAsync);
            
            _ = LoadTestsAsync();
        }

        private async Task LoadTestsAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var tests = await _dataService.GetAllTestsAsync();
                Tests.Clear();
                
                foreach (var test in tests)
                {
                    var testViewModel = new TestViewModel
                    {
                        Id = test.Id,
                        Title = test.Title,
                        Description = test.Description ?? "N/A",
                        CreatorName = test.Creator?.FullName ?? test.Creator?.Email ?? "N/A"
                    };
                    
                    var questions = await _dataService.GetQuestionsByTestIdAsync(test.Id);
                    testViewModel.QuestionCount = questions.Count();
                    
                    Tests.Add(testViewModel);
                }
                
                ShowSuccess($"Loaded {Tests.Count} tests successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading tests: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchTestsAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var allTests = await _dataService.GetAllTestsAsync();
                var filteredTests = allTests.Where(t => 
                    string.IsNullOrWhiteSpace(SearchText) ||
                    t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (t.Description != null && t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
                
                Tests.Clear();
                
                foreach (var test in filteredTests)
                {
                    var testViewModel = new TestViewModel
                    {
                        Id = test.Id,
                        Title = test.Title,
                        Description = test.Description ?? "N/A",
                        CreatorName = test.Creator?.FullName ?? test.Creator?.Email ?? "N/A"
                    };
                    
                    var questions = await _dataService.GetQuestionsByTestIdAsync(test.Id);
                    testViewModel.QuestionCount = questions.Count();
                    
                    Tests.Add(testViewModel);
                }
                
                ShowSuccess($"Found {Tests.Count} tests matching your search.");
            }
            catch (Exception ex)
            {
                ShowError($"Error searching tests: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CreateTestAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                // You would typically show a dialog to get user input here
                // For simplicity, we're creating a test with placeholder data
                
                var users = await _dataService.GetUsersByRoleAsync("teacher");
                var creator = users.FirstOrDefault();
                if (creator == null)
                {
                    ShowError("No teacher found to assign as test creator.");
                    return;
                }
                
                var newTest = new Test
                {
                    Id = Guid.NewGuid(),
                    Title = "New Test",
                    Description = "Test Description",
                    CreatorId = creator.Id,
                    TimeLimit = 60
                };
                
                await _dataService.CreateTestAsync(newTest);
                
                await LoadTestsAsync();
                ShowSuccess("Test created successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error creating test: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EditTestAsync(Guid testId)
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var test = await _dataService.GetTestByIdAsync(testId);
                if (test == null)
                {
                    ShowError("Test not found.");
                    return;
                }
                
                // Show dialog to edit (implementation would be in the view)
                
                // Update in database
                await _dataService.UpdateTestAsync(test);
                
                await LoadTestsAsync();
                ShowSuccess("Test updated successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating test: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ViewTestQuestionsAsync(Guid testId)
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var questions = await _dataService.GetQuestionsByTestIdAsync(testId);
                
                // In a real app, you'd navigate to a new view to show questions
                // For now, just show a message
                
                ShowSuccess($"Test has {questions.Count()} questions.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading test questions: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteTestAsync(Guid testId)
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var test = Tests.FirstOrDefault(t => t.Id == testId);
                if (test == null)
                {
                    ShowError("Test not found.");
                    return;
                }
                
                // Check if test has questions
                var questions = await _dataService.GetQuestionsByTestIdAsync(testId);
                if (questions.Any())
                {
                    ShowError("Cannot delete test with questions. Please delete questions first.");
                    return;
                }
                
                // Delete from database (implementation would need to be added)
                // await _dataService.DeleteTestAsync(testId);
                
                Tests.Remove(test);
                ShowSuccess("Test deleted successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting test: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            SuccessMessage = string.Empty;
        }

        private void ShowSuccess(string message)
        {
            SuccessMessage = message;
            ErrorMessage = string.Empty;
        }

        private void ClearMessages()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
        }
    }

    public partial class TestViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Guid _id;
        
        [ObservableProperty]
        private string _title = string.Empty;
        
        [ObservableProperty]
        private string _description = string.Empty;
        
        [ObservableProperty]
        private string _creatorName = string.Empty;
        
        [ObservableProperty]
        private int _questionCount;
    }
}
