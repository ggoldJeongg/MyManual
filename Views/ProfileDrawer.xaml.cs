using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Extensions.DependencyInjection;
using MyManual.Services.Interfaces;
using MyManual.ViewModels;

namespace MyManual.Views
{
    public partial class ProfileDrawer : UserControl
    {
        public event EventHandler? LogoutRequested;

        private bool _isOpen = false;
        private ProfileDrawerViewModel? _viewModel;

        public ProfileDrawer()
        {
            InitializeComponent();

            // ViewModel 생성 및 바인딩
            _viewModel = new ProfileDrawerViewModel(
                App.Services.GetRequiredService<IUserService>()
            );
            DataContext = _viewModel;

            // ViewModel의 로그아웃 이벤트 구독
            _viewModel.LogoutRequested += (s, e) =>
            {
                Close();
                LogoutRequested?.Invoke(this, EventArgs.Empty);
            };
        }

        // ==================== 애니메이션 (UI 관련이므로 코드비하인드에 유지) ====================

        public void Open()
        {
            if (_isOpen) return;
            _isOpen = true;

            _viewModel?.RefreshUserInfo();
            Overlay.Visibility = Visibility.Visible;

            var overlayAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            var drawerAnimation = new DoubleAnimation(320, 0, TimeSpan.FromMilliseconds(250))
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
            var drawerAnimation = new DoubleAnimation(0, 320, TimeSpan.FromMilliseconds(250))
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

        // ==================== UI 이벤트 핸들러 (애니메이션 관련만 유지) ====================

        private void OnOverlayClick(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
