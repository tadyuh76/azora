using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AvaloniaAzora.Models;
using System;
using System.Windows.Input;

namespace AvaloniaAzora.ViewModels.Admin
{
    public partial class UserListItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Guid _userId;
        
        [ObservableProperty]
        private string _email = string.Empty;
        
        [ObservableProperty]
        private string _fullName = string.Empty;
        
        [ObservableProperty]
        private string _role = string.Empty;
        
        [ObservableProperty]
        private string _displayName = string.Empty;
        
        [ObservableProperty]
        private string _roleColor = "#6B7280";

        public ICommand EditCommand { get; }
        public ICommand ChangeRoleCommand { get; }

        public event EventHandler<Guid>? EditRequested;
        public event EventHandler<Guid>? ChangeRoleRequested;

        public UserListItemViewModel()
        {
            EditCommand = new RelayCommand(EditUser);
            ChangeRoleCommand = new RelayCommand(ChangeRole);
        }

        public void UpdateFromUser(User user)
        {
            UserId = user.Id;
            Email = user.Email;
            FullName = user.FullName ?? "N/A";
            Role = user.Role;
            
            DisplayName = !string.IsNullOrEmpty(FullName) && FullName != "N/A" 
                ? FullName 
                : Email.Split('@')[0];
                
            RoleColor = Role.ToLower() switch
            {
                "admin" => "#8B5CF6",
                "teacher" => "#3B82F6",
                "student" => "#10B981",
                _ => "#6B7280"
            };
        }

        private void EditUser()
        {
            EditRequested?.Invoke(this, UserId);
        }

        private void ChangeRole()
        {
            ChangeRoleRequested?.Invoke(this, UserId);
        }
    }
}
