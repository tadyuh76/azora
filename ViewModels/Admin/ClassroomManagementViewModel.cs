using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Services;
using AvaloniaAzora.Models;
using System.Collections.ObjectModel;

namespace AvaloniaAzora.ViewModels.Admin
{
    public partial class ClassroomManagementViewModel : ViewModelBase
    {
        private readonly IClassroomService _classroomService;
        private readonly IUserService _userService;
        private readonly IAuditService _auditService;

        [ObservableProperty]
        private ObservableCollection<Class> _classrooms = new();

        [ObservableProperty]
        private ObservableCollection<Class> _filteredClassrooms = new();

        [ObservableProperty]
        private Class? _selectedClassroom;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private bool _isEditDialogOpen = false;

        [ObservableProperty]
        private ClassroomEditViewModel? _editClassroomViewModel;

        [ObservableProperty]
        private bool _isStudentManagementDialogOpen = false;

        [ObservableProperty]
        private StudentManagementViewModel? _studentManagementViewModel;

        public ClassroomManagementViewModel(IClassroomService classroomService, IUserService userService, IAuditService auditService)
        {
            _classroomService = classroomService;
            _userService = userService;
            _auditService = auditService;

            LoadClassroomsCommand = new AsyncRelayCommand(LoadClassroomsAsync);
            SearchClassroomsCommand = new AsyncRelayCommand(SearchClassroomsAsync);
            CreateClassroomCommand = new RelayCommand(CreateClassroom);
            EditClassroomCommand = new RelayCommand<Class>(EditClassroom);
            DeleteClassroomCommand = new AsyncRelayCommand<Class>(DeleteClassroomAsync);
            ManageStudentsCommand = new RelayCommand<Class>(ManageStudents);
            ViewClassroomDetailsCommand = new RelayCommand<Class>(ViewClassroomDetails);
            
            _ = LoadClassroomsAsync();
        }

        partial void OnSearchTextChanged(string value)
        {
            _ = SearchClassroomsAsync();
        }

        public IAsyncRelayCommand LoadClassroomsCommand { get; }
        public IAsyncRelayCommand SearchClassroomsCommand { get; }
        public IRelayCommand CreateClassroomCommand { get; }
        public IRelayCommand<Class> EditClassroomCommand { get; }
        public IAsyncRelayCommand<Class> DeleteClassroomCommand { get; }
        public IRelayCommand<Class> ManageStudentsCommand { get; }
        public IRelayCommand<Class> ViewClassroomDetailsCommand { get; }

        private async Task LoadClassroomsAsync()
        {
            try
            {
                IsLoading = true;
                var classrooms = await _classroomService.GetAllClassroomsAsync();
                
                Classrooms.Clear();
                foreach (var classroom in classrooms)
                {
                    Classrooms.Add(classroom);
                }

                await SearchClassroomsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading classrooms: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchClassroomsAsync()
        {
            try
            {
                var classrooms = string.IsNullOrWhiteSpace(SearchText) 
                    ? Classrooms 
                    : await _classroomService.SearchClassroomsAsync(SearchText);

                FilteredClassrooms.Clear();
                foreach (var classroom in classrooms)
                {
                    FilteredClassrooms.Add(classroom);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching classrooms: {ex.Message}");
            }
        }

        private void CreateClassroom()
        {
            EditClassroomViewModel = new ClassroomEditViewModel(_classroomService, _userService, null);
            EditClassroomViewModel.ClassroomSaved += OnClassroomSaved;
            IsEditDialogOpen = true;
        }

        private void EditClassroom(Class? classroom)
        {
            if (classroom == null) return;

            EditClassroomViewModel = new ClassroomEditViewModel(_classroomService, _userService, classroom);
            EditClassroomViewModel.ClassroomSaved += OnClassroomSaved;
            IsEditDialogOpen = true;
        }

        private async Task DeleteClassroomAsync(Class? classroom)
        {
            if (classroom == null) return;

            try
            {
                var success = await _classroomService.DeleteClassroomAsync(classroom.ClassId);
                if (success)
                {
                    await LoadClassroomsAsync();
                }
            }
            catch (Exception ex)
            {
                // Show error notification
                System.Diagnostics.Debug.WriteLine($"Error deleting classroom: {ex.Message}");
            }
        }

        private void ManageStudents(Class? classroom)
        {
            if (classroom == null) return;

            StudentManagementViewModel = new StudentManagementViewModel(_classroomService, _userService, classroom);
            StudentManagementViewModel.StudentsUpdated += OnStudentsUpdated;
            IsStudentManagementDialogOpen = true;
        }

        private void ViewClassroomDetails(Class? classroom)
        {
            if (classroom == null) return;
            
            // Navigate to detailed classroom view
            // This could open a new window or navigate to a details page
            System.Diagnostics.Debug.WriteLine($"Viewing details for classroom: {classroom.ClassName}");
        }

        private async void OnClassroomSaved(object? sender, EventArgs e)
        {
            IsEditDialogOpen = false;
            EditClassroomViewModel = null;
            await LoadClassroomsAsync();
        }

        private async void OnStudentsUpdated(object? sender, EventArgs e)
        {
            IsStudentManagementDialogOpen = false;
            StudentManagementViewModel = null;
            await LoadClassroomsAsync();
        }
    }
}
