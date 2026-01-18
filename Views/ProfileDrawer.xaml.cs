using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Extensions.DependencyInjection;
using MyManual.Models;
using MyManual.Services.Interfaces;

namespace MyManual.Views
{
    public partial class ProfileDrawer : UserControl
    {
        public event EventHandler? LogoutRequested;

        private bool _isOpen = false;

        public ProfileDrawer()
        {
            InitializeComponent();
            UpdateUserInfo();
        }

        public void Open()
        {
            if (_isOpen) return;
            _isOpen = true;

            UpdateUserInfo();
            LoadUsersIfAdmin();
            Overlay.Visibility = Visibility.Visible;

            var overlayAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            var drawerAnimation = new DoubleAnimation(320, 0, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            Overlay.BeginAnimation(OpacityProperty, overlayAnimation);
            DrawerTransform.BeginAnimation(TranslateTransform.XProperty, drawerAnimation);
        }

        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;

            var overlayAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            var drawerAnimation = new DoubleAnimation(0, 320, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            overlayAnimation.Completed += (s, e) => Overlay.Visibility = Visibility.Collapsed;

            Overlay.BeginAnimation(OpacityProperty, overlayAnimation);
            DrawerTransform.BeginAnimation(TranslateTransform.XProperty, drawerAnimation);
        }

        public void Toggle()
        {
            if (_isOpen)
                Close();
            else
                Open();
        }

        private void UpdateUserInfo()
        {
            var user = App.CurrentUser;
            if (user != null)
            {
                UserNameText.Text = user.Name;
                AvatarText.Text = user.Name.Length > 0 ? user.Name[0].ToString() : "?";
                JoinDateText.Text = $"입사일: {user.JoinDate:yyyy년 M월 d일}";

                if (user.IsAdmin)
                {
                    RoleText.Text = "관리자";
                    RoleBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF3E0"));
                    RoleText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
                    AdminSection.Visibility = Visibility.Visible;
                }
                else
                {
                    RoleText.Text = "일반 사용자";
                    RoleBadge.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
                    RoleText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1976D2"));
                    AdminSection.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void LoadUsersIfAdmin()
        {
            var user = App.CurrentUser;
            if (user == null || !user.IsAdmin) return;

            try
            {
                var userService = App.Services.GetRequiredService<IUserService>();
                var users = userService.GetAllUsers();

                var userViewModels = users.Select(u => new UserListItemViewModel(u)).ToList();
                UserListControl.ItemsSource = userViewModels;
                UserCountText.Text = $"({users.Count}명)";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[사용자 목록 로드 실패] {ex.Message}");
            }
        }

        private void OnOverlayClick(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            Close();
            LogoutRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnRefreshUsersClick(object sender, RoutedEventArgs e)
        {
            LoadUsersIfAdmin();
        }

        private void OnToggleAdminClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is UserListItemViewModel userVM)
            {
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
                        var userService = App.Services.GetRequiredService<IUserService>();
                        if (userService.SetAdminStatus(userVM.Id, newAdminStatus))
                        {
                            LoadUsersIfAdmin();
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
    }

    // 사용자 목록 아이템 ViewModel
    public class UserListItemViewModel
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
