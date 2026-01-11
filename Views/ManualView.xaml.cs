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

        // 온보딩 버튼 클릭 시 MainWindow의 NavigateToOnboarding 호출
        private void OnOnboardingButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToOnboarding();
        }
    }
}
