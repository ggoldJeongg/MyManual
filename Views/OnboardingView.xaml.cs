using System.Windows;
using System.Windows.Controls;

namespace MyManual.Views
{
    public partial class OnboardingView : UserControl
    {
        public OnboardingView()
        {
            InitializeComponent();
        }

        private void OnManualButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToManual();
        }

        private void OnCategoryButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToCategoryMenu();
        }
    }
}
