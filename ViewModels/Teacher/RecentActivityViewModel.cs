using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaAzora.ViewModels
{
    public partial class RecentActivityViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _activityTitle = string.Empty;

        [ObservableProperty]
        private string _activityDescription = string.Empty;

        [ObservableProperty]
        private string _timeAgo = string.Empty;

        [ObservableProperty]
        private string _activityColor = "#6B7280";
    }
}