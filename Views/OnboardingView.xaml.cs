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

        // 매뉴얼 버튼 클릭 시 MainWindow의 NavigateToManual 호출
        private void OnManualButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToManual();
        }
    }
}
