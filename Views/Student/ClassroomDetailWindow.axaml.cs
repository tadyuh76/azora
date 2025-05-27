using Avalonia.Controls;
using AvaloniaAzora.ViewModels.Student;
using System;
using System.Threading.Tasks;

namespace AvaloniaAzora.Views.Student
{
    public partial class ClassroomDetailWindow : Window
    {
        public ClassroomDetailWindow()
        {
            InitializeComponent();
        }

        public ClassroomDetailWindow(Guid classId, Guid userId) : this()
        {
            try
            {
                Console.WriteLine($"🏗️ Initializing ClassroomDetailWindow for class: {classId}, user: {userId}");

                var viewModel = new ClassroomDetailViewModel();
                DataContext = viewModel;

                // Handle back button click
                var backButton = this.FindControl<Button>("BackButton");
                if (backButton != null)
                {
                    backButton.Click += (sender, e) => Close();
                }

                Console.WriteLine("✅ ViewModel created and DataContext set");

                // Load data when window is opened
                Opened += async (sender, e) => await LoadDataAsync(viewModel, classId, userId);

                Console.WriteLine("✅ Opened event handler attached");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in ClassroomDetailWindow constructor: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private async Task LoadDataAsync(ClassroomDetailViewModel viewModel, Guid classId, Guid userId)
        {
            try
            {
                Console.WriteLine($"📚 Loading classroom details for class: {classId}, user: {userId}");
                await viewModel.LoadClassroomDetailsAsync(classId, userId);
                Console.WriteLine("✅ Classroom details loading completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading classroom details: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // You could show an error dialog here
            }
        }
    }
}