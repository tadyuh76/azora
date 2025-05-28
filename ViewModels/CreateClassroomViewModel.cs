using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaAzora.ViewModels
{
    public partial class CreateClassroomViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _className = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;
    }
}