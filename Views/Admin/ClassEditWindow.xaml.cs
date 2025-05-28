using Avalonia.Controls;
using AvaloniaAzora.ViewModels.Admin;

namespace AvaloniaAzora.Views.Admin
{
    public partial class ClassEditWindow : Window
    {
        public ClassEditWindow()
        {
            InitializeComponent();
            DataContext = new ClassEditViewModel();
        }
    }
}
