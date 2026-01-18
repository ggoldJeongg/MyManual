using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using MyManual.Services.Interfaces;

namespace MyManual.Views
{
    public partial class OnboardingView : UserControl
    {
        private INavigationService Navigation => App.Services.GetRequiredService<INavigationService>();

        public OnboardingView()
        {
            InitializeComponent();
        }

        private void OnManualButtonClick(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateToManual();
        }

        private void OnCategoryButtonClick(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateToCategoryMenu();
        }

        private void OnOnboardingManageClick(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateToOnboardingManage();
        }

        private void OnUserNameClick(object sender, RoutedEventArgs e)
        {
            ProfileDrawer.Open();
        }

        private void OnLogoutRequested(object? sender, EventArgs e)
        {
            Navigation.NavigateToUserInit();
        }
    }
}
