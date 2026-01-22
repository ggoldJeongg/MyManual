using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MyManual.Views
{
    public partial class Header : UserControl
    {
        public static readonly DependencyProperty ActiveMenuProperty =
            DependencyProperty.Register(nameof(ActiveMenu), typeof(string), typeof(Header),
                new PropertyMetadata("Onboarding", OnActiveMenuChanged));

        public static readonly DependencyProperty RightContentProperty =
            DependencyProperty.Register(nameof(RightContent), typeof(object), typeof(Header),
                new PropertyMetadata(null));

        public string ActiveMenu
        {
            get => (string)GetValue(ActiveMenuProperty);
            set => SetValue(ActiveMenuProperty, value);
        }

        public object RightContent
        {
            get => GetValue(RightContentProperty);
            set => SetValue(RightContentProperty, value);
        }

        public event RoutedEventHandler? OnboardingClick;
        public event RoutedEventHandler? ManualClick;
        public event RoutedEventHandler? CategoryClick;
        public event RoutedEventHandler? OnboardingManageClick;
        public event RoutedEventHandler? UserNameClick;

        public Header()
        {
            InitializeComponent();
            UpdateMenuStyles();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 화면에 표시될 때마다 최신 사용자 정보로 갱신
            UpdateUserName();
            UpdateAdminMenuVisibility();
        }

        private void UpdateUserName()
        {
            var user = App.CurrentUser;
            UserNameText.Text = user?.Name ?? "사용자";
        }

        private void UpdateAdminMenuVisibility()
        {
            var user = App.CurrentUser;
            var isAdmin = user?.IsAdmin == true;

            // 관리자: 온보딩 관리 버튼 표시, 온보딩 버튼 숨김
            OnboardingManageButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            OnboardingButton.Visibility = isAdmin ? Visibility.Collapsed : Visibility.Visible;
        }

        private static void OnActiveMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Header header)
            {
                header.UpdateMenuStyles();
            }
        }

        private void UpdateMenuStyles()
        {
            var activeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00A878"));
            var inactiveColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
            var boldWeight = FontWeights.Bold;
            var normalWeight = FontWeights.Normal;

            OnboardingButton.Foreground = ActiveMenu == "Onboarding" ? activeColor : inactiveColor;
            OnboardingButton.FontWeight = ActiveMenu == "Onboarding" ? boldWeight : normalWeight;

            ManualButton.Foreground = ActiveMenu == "Manual" ? activeColor : inactiveColor;
            ManualButton.FontWeight = ActiveMenu == "Manual" ? boldWeight : normalWeight;

            CategoryButton.Foreground = ActiveMenu == "Category" ? activeColor : inactiveColor;
            CategoryButton.FontWeight = ActiveMenu == "Category" ? boldWeight : normalWeight;

            OnboardingManageButton.Foreground = ActiveMenu == "OnboardingManage" ? activeColor : inactiveColor;
            OnboardingManageButton.FontWeight = ActiveMenu == "OnboardingManage" ? boldWeight : normalWeight;
        }

        private void OnOnboardingButtonClick(object sender, RoutedEventArgs e)
        {
            OnboardingClick?.Invoke(this, e);
        }

        private void OnManualButtonClick(object sender, RoutedEventArgs e)
        {
            ManualClick?.Invoke(this, e);
        }

        private void OnCategoryButtonClick(object sender, RoutedEventArgs e)
        {
            CategoryClick?.Invoke(this, e);
        }

        private void OnOnboardingManageButtonClick(object sender, RoutedEventArgs e)
        {
            OnboardingManageClick?.Invoke(this, e);
        }

        private void OnUserNameClick(object sender, MouseButtonEventArgs e)
        {
            UserNameClick?.Invoke(this, e);
        }
    }
}
