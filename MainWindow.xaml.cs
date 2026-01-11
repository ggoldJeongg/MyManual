using System.Windows;
using MyManual.ViewModels;
using MyManual.Views;

namespace MyManual;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // App.xaml.cs에서 설정한 CurrentUser 사용
        if (App.CurrentUser != null)
        {
            var onboardingView = new OnboardingView
            {
                DataContext = new OnboardingViewModel(App.CurrentUser)
            };

            this.Content = onboardingView;
        }
    }
}
