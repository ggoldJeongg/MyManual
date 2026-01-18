using System;
using System.Windows;
using System.Windows.Controls;

namespace MyManual.Views
{
    public partial class ManualView : UserControl
    {
        public ManualView()
        {
            InitializeComponent();
        }

        private void OnOnboardingButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToOnboarding();
        }

        private void OnCategoryButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToCategoryMenu();
        }

        private void OnManualCreateButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToManualCreate();
        }

        private void OnUserNameClick(object sender, RoutedEventArgs e)
        {
            ProfileDrawer.Open();
        }

        private void OnLogoutRequested(object? sender, EventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToUserInit();
        }
    }
}
