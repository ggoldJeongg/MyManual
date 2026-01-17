using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using MyManual.Services.Interfaces;

namespace MyManual.Views
{
    public partial class ManualCreateView : UserControl
    {
        private INavigationService Navigation => App.Services.GetRequiredService<INavigationService>();

        public ManualCreateView()
        {
            InitializeComponent();
        }

        private void OnManualButtonClick(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateToManual();
        }

        private void OnOnboardingButtonClick(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateToOnboarding();
        }
    }
}
