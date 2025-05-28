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
    public partial class ClassManagementViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        
        [ObservableProperty]
        private string _searchText = string.Empty;
        
        [ObservableProperty]
        private bool _showActiveOnly = true;
        
        [ObservableProperty]
        private bool _isLoading = false;
        
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        [ObservableProperty]
        private string _successMessage = string.Empty;
        
        [ObservableProperty]
        private ClassListItemViewModel? _selectedClass;
        
        [ObservableProperty]
        private int _totalClasses = 0;
        
        [ObservableProperty]
        private int _activeClasses = 0;
        
        [ObservableProperty]
        private int _archivedClasses = 0;

        public ObservableCollection<ClassListItemViewModel> Classes { get; } = new();

        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CreateClassCommand { get; }
        public ICommand EditClassCommand { get; }
        public ICommand ViewEnrollmentsCommand { get; }
        public ICommand ArchiveClassCommand { get; }
        public ICommand DeleteClassCommand { get; }

        public event EventHandler<Guid>? CreateClassRequested;
        public event EventHandler<Guid>? EditClassRequested;
        public event EventHandler<Guid>? ViewEnrollmentsRequested;

        public ClassManagementViewModel()
        {
           _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            
            SearchCommand = new AsyncRelayCommand(SearchClassesAsync);
            RefreshCommand = new AsyncRelayCommand(LoadClassesAsync);
            CreateClassCommand = new RelayCommand(CreateClass);
            
            // Load classes on initialization
            _ = LoadClassesAsync();
        }

        public async Task LoadClassesAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var classes = await _dataService.GetAllClassesAsync();
                Classes.Clear();
                
                foreach (var classEntity in classes)
                {
                    var classViewModel = new ClassListItemViewModel();
                    classViewModel.UpdateFromClass(classEntity);
                    
                    // Load additional data
                    classViewModel.StudentCount = await _dataService.GetClassEnrollmentCountAsync(classEntity.Id);
                    var tests = await _dataService.GetTestsByClassIdAsync(classEntity.Id);
                    classViewModel.TestCount = tests.Count();
                    
                    // Subscribe to events
                    classViewModel.EditRequested += (s, classId) => EditClassRequested?.Invoke(this, classId);
                    classViewModel.ViewEnrollmentsRequested += (s, classId) => ViewEnrollmentsRequested?.Invoke(this, classId);
                    classViewModel.DeleteRequested += (s, classId) => _ = DeleteClassAsync(classId);
                    
                    Classes.Add(classViewModel);
                }
                
                await UpdateStatisticsAsync();
                ShowSuccess($"Loaded {Classes.Count} classes successfully.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading classes: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchClassesAsync()
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                bool? activeFilter = ShowActiveOnly ? true : null;
                var classes = await _dataService.SearchClassesAsync(SearchText, activeFilter);
                
                Classes.Clear();
                
                foreach (var classEntity in classes)
                {
                    var classViewModel = new ClassListItemViewModel();
                    classViewModel.UpdateFromClass(classEntity);
                    
                    // Load additional data
                    classViewModel.StudentCount = await _dataService.GetClassEnrollmentCountAsync(classEntity.Id);
                    var tests = await _dataService.GetTestsByClassIdAsync(classEntity.Id);
                    classViewModel.TestCount = tests.Count();
                    
                    // Subscribe to events
                    classViewModel.EditRequested += (s, classId) => EditClass(classId);
                    classViewModel.ViewEnrollmentsRequested += (s, classId) => ViewEnrollments(classId);
                    classViewModel.ArchiveRequested += (s, classId) => _ = ArchiveClassAsync(classId);
                    classViewModel.DeleteRequested += (s, classId) => _ = DeleteClassAsync(classId);
                    
                    Classes.Add(classViewModel);
                }
                
                ShowSuccess($"Found {Classes.Count} classes matching your search.");
            }
            catch (Exception ex)
            {
                ShowError($"Error searching classes: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateStatisticsAsync()
        {
            try
            {
                TotalClasses = await _dataService.GetTotalClassesCountAsync();
                ActiveClasses = await _dataService.GetActiveClassesCountAsync();
                ArchivedClasses = TotalClasses - ActiveClasses;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating class statistics: {ex.Message}");
            }
        }

        private void CreateClass()
        {
            CreateClassRequested?.Invoke(this, Guid.Empty);
        }

        private void EditClass(Guid classId)
        {
            EditClassRequested?.Invoke(this, classId);
        }

        private void ViewEnrollments(Guid classId)
        {
            ViewEnrollmentsRequested?.Invoke(this, classId);
        }

        private async Task ArchiveClassAsync(Guid classId)
        {
            try
            {
                IsLoading = true;
                ClearMessages();

                var success = await _dataService.ArchiveClassAsync(classId);
                if (success)
                {
                    ShowSuccess("Class archived successfully.");
                    await LoadClassesAsync();
                }
                else
                {
                    ShowError("Failed to archive class.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error archiving class: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteClassAsync(Guid classId)
        {
            try
            {
                IsLoading = true;
                ClearMessages();

                var success = await _dataService.DeleteClassAsync(classId);
                if (success)
                {
                    ShowSuccess("Class deleted/archived successfully.");
                    await LoadClassesAsync();
                }
                else
                {
                    ShowError("Failed to delete class.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting class: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnShowActiveOnlyChanged(bool value)
        {
            _ = LoadClassesAsync();
        }

        partial void OnSearchTextChanged(string value)
        {
            _ = SearchClassesAsync();
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
}
