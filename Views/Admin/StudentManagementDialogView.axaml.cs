using Avalonia.Controls;
using AvaloniaAzora.ViewModels.Admin; // Assuming ViewModelBase is here

namespace AvaloniaAzora.Views.Admin
{
    public partial class StudentManagementDialogView : UserControl
    {
        public StudentManagementDialogView()
        {
            InitializeComponent();
        }

        // Optional: If you need to pass the ViewModel via constructor from code
        public StudentManagementDialogView(StudentManagementViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
