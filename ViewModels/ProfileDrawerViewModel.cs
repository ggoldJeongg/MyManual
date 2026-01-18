using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MyManual.Commands;
using MyManual.Models;
using MyManual.Services.Interfaces;
using MyManual.ViewModels.Base;

namespace MyManual.ViewModels
{
    public class ProfileDrawerViewModel : ViewModelBase
    {
        // ==================== Services ====================

        private readonly IUserService _userService;

        // ==================== 현재 사용자 정보 ====================

        public string UserName => App.CurrentUser?.Name ?? "사용자";
        public string AvatarInitial => UserName.Length > 0 ? UserName[0].ToString() : "?";
        public string JoinDateText => App.CurrentUser != null
            ? $"입사일: {App.CurrentUser.JoinDate:yyyy년 M월 d일}"
            : "";

        public bool IsAdmin => App.CurrentUser?.IsAdmin ?? false;

        public string RoleText => IsAdmin ? "관리자" : "일반 사용자";

        public Brush RoleBadgeBackground => IsAdmin
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));

        public Brush RoleBadgeForeground => IsAdmin
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));

        // ==================== 사용자 목록 (관리자용) ====================

        private ObservableCollection<UserListItemViewModel> _users = new();
        public ObservableCollection<UserListItemViewModel> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        public string UserCountText => $"({Users.Count}명)";

        // ==================== Commands ====================

        public ICommand RefreshUsersCommand { get; }
        public ICommand ToggleAdminCommand { get; }
        public ICommand LogoutCommand { get; }

        // ==================== Events ====================

        public event EventHandler? LogoutRequested;

        // ==================== 생성자 ====================

        public ProfileDrawerViewModel(IUserService userService)
        {
            _userService = userService;

            RefreshUsersCommand = new RelayCommand(_ => LoadUsers());
            ToggleAdminCommand = new RelayCommand(OnToggleAdmin);
            LogoutCommand = new RelayCommand(_ => LogoutRequested?.Invoke(this, EventArgs.Empty));
        }

        // ==================== Public Methods ====================

        public void RefreshUserInfo()
        {
            OnPropertyChanged(nameof(UserName));
            OnPropertyChanged(nameof(AvatarInitial));
            OnPropertyChanged(nameof(JoinDateText));
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(RoleText));
            OnPropertyChanged(nameof(RoleBadgeBackground));
            OnPropertyChanged(nameof(RoleBadgeForeground));

            if (IsAdmin)
            {
                LoadUsers();
            }
        }

        // ==================== Private Methods ====================

        private void LoadUsers()
        {
            if (!IsAdmin) return;

            try
            {
                var users = _userService.GetAllUsers();
                var userVMs = users.Select(u => new UserListItemViewModel(u)).ToList();
                Users = new ObservableCollection<UserListItemViewModel>(userVMs);
                OnPropertyChanged(nameof(UserCountText));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[사용자 목록 로드 실패] {ex.Message}");
            }
        }

        private void OnToggleAdmin(object? parameter)
        {
            if (parameter is not UserListItemViewModel userVM) return;

            var currentUser = App.CurrentUser;
            if (currentUser == null || !currentUser.IsAdmin)
            {
                MessageBox.Show("관리자 권한이 필요합니다.", "권한 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (userVM.Id == currentUser.Id)
            {
                MessageBox.Show("자신의 관리자 권한은 변경할 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newAdminStatus = !userVM.IsAdmin;
            var action = newAdminStatus ? "부여" : "해제";
            var result = MessageBox.Show(
                $"'{userVM.Name}' 사용자의 관리자 권한을 {action}하시겠습니까?",
                "관리자 권한 변경",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (_userService.SetAdminStatus(userVM.Id, newAdminStatus))
                    {
                        LoadUsers();
                        MessageBox.Show(
                            $"'{userVM.Name}' 사용자의 관리자 권한이 {action}되었습니다.",
                            "완료",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("사용자를 찾을 수 없습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[관리자 권한 변경 실패] {ex.Message}");
                    MessageBox.Show($"권한 변경 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    /// <summary>
    /// 사용자 목록 아이템 ViewModel
    /// </summary>
    public class UserListItemViewModel : ViewModelBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AvatarInitial { get; set; }
        public string JoinDateText { get; set; }
        public string RoleText { get; set; }
        public bool IsAdmin { get; set; }
        public bool CanToggleAdmin { get; set; }

        // UI 바인딩용 Brush
        public Brush AvatarBackground { get; set; }
        public Brush AvatarForeground { get; set; }
        public Brush RoleBadgeBackground { get; set; }
        public Brush RoleBadgeForeground { get; set; }

        public UserListItemViewModel(User user)
        {
            Id = user.Id;
            Name = user.Name;
            AvatarInitial = user.Name.Length > 0 ? user.Name[0].ToString() : "?";
            JoinDateText = $"입사: {user.JoinDate:yyyy.MM.dd}";
            IsAdmin = user.IsAdmin;

            // 현재 로그인한 사용자가 자기 자신이 아닌 경우에만 토글 가능
            var currentUser = App.CurrentUser;
            CanToggleAdmin = currentUser != null && currentUser.Id != user.Id;

            if (user.IsAdmin)
            {
                RoleText = "관리자";
                AvatarBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0"));
                AvatarForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
                RoleBadgeBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0"));
                RoleBadgeForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
            }
            else
            {
                RoleText = "사용자";
                AvatarBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));
                AvatarForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#43A047"));
                RoleBadgeBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
                RoleBadgeForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));
            }
        }
    }
}
