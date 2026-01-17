using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using MyManual.Services.Interfaces;

namespace MyManual.Views
{
    public partial class ManualView : UserControl
    {
        private INavigationService Navigation => App.Services.GetRequiredService<INavigationService>();

        public ManualView()
        {
            InitializeComponent();
        }

        private void OnOnboardingButtonClick(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateToOnboarding();
        }

        private void OnCategoryButtonClick(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateToCategoryMenu();
        }

        private void OnManualCreateButtonClick(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateToManualCreate();
        }

        private void OnManualEditButtonClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.ManualViewModel vm && vm.SelectedManual != null)
            {
                Navigation.NavigateToManualEdit(vm.SelectedManual.Id);
            }
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
