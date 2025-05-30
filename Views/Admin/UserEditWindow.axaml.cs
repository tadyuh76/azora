using Avalonia.Controls;
using AvaloniaAzora.ViewModels.Admin;

namespace AvaloniaAzora.Views.Admin
{
    public partial class UserEditWindow : Window
    {
        public UserEditWindow()
        {
            InitializeComponent();
            DataContext = new UserEditViewModel();
        }
    }
}
