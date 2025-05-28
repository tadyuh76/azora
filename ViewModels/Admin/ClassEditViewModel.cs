using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Models;
using AvaloniaAzora.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels.Admin
{
    public partial class ClassEditViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        
        [ObservableProperty]
        private Guid? _classId;
        
        [ObservableProperty]
        private string _className = string.Empty;
        
        [ObservableProperty]
        private string _description = string.Empty;
        
        [ObservableProperty]
        private Guid? _selectedTeacherId;
        
        [ObservableProperty]
        private bool _isActive = true;
        
        [ObservableProperty]
        private bool _isEditMode = false;
        
        [ObservableProperty]
        private bool _isLoading = false;
        
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        [ObservableProperty]
        private string _successMessage = string.Empty;

        public ObservableCollection<TeacherOptionViewModel> AvailableTeachers { get; } = new();

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler? ClassSaved;
        public event EventHandler? Cancelled;

        public ClassEditViewModel()
        {
           _dataService = AvaloniaAzora.Services.ServiceProvider.Instance.GetRequiredService<IDataService>();
            
            SaveCommand = new AsyncRelayCommand(SaveClassAsync);
            CancelCommand = new RelayCommand(Cancel);
            
            _ = LoadTeachersAsync();
        }

        public void SetupForCreate()
        {
            ClassId = null;
            IsEditMode = false;
            ClassName = string.Empty;
            Description = string.Empty;
            SelectedTeacherId = null;
            IsActive = true;
            ClearMessages();
        }

        public async Task SetupForEditAsync(Guid classId)
        {
            try
            {
                IsLoading = true;
                ClearMessages();
                
                var classEntity = await _dataService.GetClassByIdAsync(classId);
                if (classEntity != null)
                {
                    ClassId = classEntity.Id;
                    IsEditMode = true;
                    ClassName = classEntity.ClassName;
                    Description = classEntity.Description ?? string.Empty;
                    SelectedTeacherId = classEntity.TeacherId;
                }
                else
                {
                    ShowError("Class not found.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading class: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadTeachersAsync()
        {
            try
            {
                var teachers = await _dataService.GetUsersByRoleAsync("teacher");
                AvailableTeachers.Clear();
                
                // Add "No Teacher" option
                AvailableTeachers.Add(new TeacherOptionViewModel
                {
                    Id = null,
                    Name = "No Teacher Assigned"
                });
                
                foreach (var teacher in teachers)
                {
                    AvailableTeachers.Add(new TeacherOptionViewModel
                    {
                        Id = teacher.Id,
                        Name = teacher.FullName ?? teacher.Email
                    });
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading teachers: {ex.Message}");
            }
        }

        private async Task SaveClassAsync()
        {
             if (!ValidateInput()) return;

            try
            {
                IsLoading = true;
                ClearMessages();

                var classEntity = new Class
                {
                    Id = ClassId ?? Guid.NewGuid(),
                    ClassName = ClassName,
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
                    TeacherId = SelectedTeacherId
                };

                if (IsEditMode)
                {
                    await _dataService.UpdateClassAsync(classEntity);
                    ShowSuccess("Class updated successfully!");
                }
                else
                {
                    await _dataService.CreateClassAsync(classEntity);
                    ShowSuccess("Class created successfully!");
                }
                
                ClassSaved?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ShowError($"Error saving class: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(ClassName))
            {
                ShowError("Class name is required.");
                return false;
            }

            return true;
        }

        private void Cancel()
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
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

    public partial class TeacherOptionViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Guid? _id;
        
        [ObservableProperty]
        private string _name = string.Empty;
    }
}
