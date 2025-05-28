using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaAzora.ViewModels
{
    public partial class CreateTestViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private int _timeLimit = 0;
    }
}