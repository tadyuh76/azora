using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Models;
using System;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels.Admin
{
    public partial class ClassListItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Guid _classId;
        
        [ObservableProperty]
        private string _className = string.Empty;
        
        [ObservableProperty]
        private string _description = string.Empty;
        
        [ObservableProperty]
        private string _teacherName = string.Empty;
        
        [ObservableProperty]
        private int _studentCount;
        
        [ObservableProperty]
        private int _testCount;
        
        [ObservableProperty]
        private bool _isActive = true;
        
        [ObservableProperty]
        private DateTimeOffset _createdAt;
        
        [ObservableProperty]
        private string _statusColor = "#10B981";
        
        [ObservableProperty]
        private string _displayDescription = string.Empty;

        public ICommand EditCommand { get; }
        public ICommand ViewEnrollmentsCommand { get; }
        public ICommand ArchiveCommand { get; }
        public ICommand DeleteCommand { get; }

        public event EventHandler<Guid>? EditRequested;
        public event EventHandler<Guid>? ViewEnrollmentsRequested;
        public event EventHandler<Guid>? ArchiveRequested;
        public event EventHandler<Guid>? DeleteRequested;

        public ClassListItemViewModel()
        {
             EditCommand = new RelayCommand(EditClass);
            ViewEnrollmentsCommand = new RelayCommand(ViewEnrollments);
            DeleteCommand = new RelayCommand(DeleteClass);
        }

        public void UpdateFromClass(Class classEntity)
        {
            ClassId = classEntity.Id;
            ClassName = classEntity.ClassName;
            Description = classEntity.Description ?? string.Empty;
            TeacherName = classEntity.Teacher?.FullName ?? classEntity.Teacher?.Email ?? "No Teacher";
            CreatedAt = classEntity.CreatedAt;
            
            DisplayDescription = string.IsNullOrEmpty(Description) ? "No description available" : Description;
        }

        private void EditClass()
        {
            EditRequested?.Invoke(this, ClassId);
        }

        private void ViewEnrollments()
        {
            ViewEnrollmentsRequested?.Invoke(this, ClassId);
        }

        private void ArchiveClass()
        {
            ArchiveRequested?.Invoke(this, ClassId);
        }

        private void DeleteClass()
        {
            DeleteRequested?.Invoke(this, ClassId);
        }
    }
}
