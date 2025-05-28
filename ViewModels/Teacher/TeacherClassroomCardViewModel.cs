using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels
{
    public partial class TeacherClassroomCardViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Guid _classId;

        [ObservableProperty]
        private string _className = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private int _studentCount;

        [ObservableProperty]
        private int _testCount;

        [ObservableProperty]
        private string _createdDate = string.Empty;

        [ObservableProperty]
        private string _subjectColor = "#6B7280";

        public ICommand ViewClassCommand { get; }
        public ICommand ManageClassCommand { get; }

        public TeacherClassroomCardViewModel()
        {
            ViewClassCommand = new RelayCommand(ViewClass);
            ManageClassCommand = new RelayCommand(ManageClass);
        }

        private void ViewClass()
        {
            // TODO: Navigate to class detail view
            Console.WriteLine($"👁️ Viewing class: {ClassName}");
        }

        private void ManageClass()
        {
            // TODO: Navigate to class management view
            Console.WriteLine($"⚙️ Managing class: {ClassName}");
        }
    }
}