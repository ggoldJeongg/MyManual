using System.Windows;
using System.Windows.Controls;

namespace MyManual.Views
{
    public partial class ManualCreateView : UserControl
    {
        public ManualCreateView()
        {
            InitializeComponent();
        }

        private void OnManualButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToManual();
        }

        private void OnOnboardingButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToOnboarding();
        }
    }
}
