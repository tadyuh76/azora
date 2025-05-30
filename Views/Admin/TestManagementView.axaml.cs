using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
