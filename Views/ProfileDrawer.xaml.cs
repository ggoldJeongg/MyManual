using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

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
            Overlay.Visibility = Visibility.Visible;

            var overlayAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            var drawerAnimation = new DoubleAnimation(300, 0, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            Overlay.BeginAnimation(OpacityProperty, overlayAnimation);
            DrawerTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, drawerAnimation);
        }

        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;

            var overlayAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            var drawerAnimation = new DoubleAnimation(0, 300, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            overlayAnimation.Completed += (s, e) => Overlay.Visibility = Visibility.Collapsed;

            Overlay.BeginAnimation(OpacityProperty, overlayAnimation);
            DrawerTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, drawerAnimation);
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
                RoleText.Text = user.IsAdmin ? "관리자" : "일반 사용자";
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
    }
}
