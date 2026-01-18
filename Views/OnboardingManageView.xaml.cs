using System;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using MyManual.Services.Interfaces;
using MyManual.ViewModels;

namespace MyManual.Views
{
    public partial class OnboardingManageView : UserControl
    {
        public OnboardingManageView()
        {
            InitializeComponent();

            // ViewModel 생성 및 바인딩
            DataContext = new OnboardingManageViewModel(
                App.Services.GetRequiredService<IUserService>(),
                App.Services.GetRequiredService<IOnboardingService>(),
                App.Services.GetRequiredService<IManualService>()
            );

            // 헤더 네비게이션 이벤트
            var navigation = App.Services.GetRequiredService<INavigationService>();
            HeaderControl.OnboardingClick += (s, e) => navigation.NavigateToOnboarding();
            HeaderControl.ManualClick += (s, e) => navigation.NavigateToManual();
            HeaderControl.CategoryClick += (s, e) => navigation.NavigateToCategoryMenu();
            HeaderControl.UserNameClick += (s, e) => ProfileDrawer.Toggle();
            ProfileDrawer.LogoutRequested += OnLogoutRequested;
        }

        private void OnLogoutRequested(object? sender, EventArgs e)
        {
            var navigationService = App.Services.GetRequiredService<INavigationService>();
            navigationService.NavigateToUserInit();
        }

        /// <summary>
        /// 외부에서 호출하여 데이터 새로고침
        /// </summary>
        public void Refresh()
        {
            if (DataContext is OnboardingManageViewModel vm)
            {
                vm.Refresh();
            }
        }
    }
}
