using Avalonia.Controls;
using AvaloniaAzora.ViewModels.Admin;

namespace AvaloniaAzora.Views.Admin
{
    public partial class TestManagementView : UserControl
    {
        public TestManagementView()
        {
            InitializeComponent();
        }

        public TestManagementView(TestManagementViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
