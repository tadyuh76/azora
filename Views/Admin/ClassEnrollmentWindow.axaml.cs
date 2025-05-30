using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaAzora.ViewModels.Admin;
using System;

namespace AvaloniaAzora.Views.Admin
{
    public partial class ClassEnrollmentWindow : Window
    {
        public ClassEnrollmentWindow(Guid classId)
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = new ClassEnrollmentViewModel(classId);
        }
    }
}
