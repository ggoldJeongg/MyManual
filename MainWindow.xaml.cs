using System.Windows;
using MyManual.Models.User;
using MyManual.ViewModels;
using MyManual.Views;

namespace MyManual;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // 테스트 사용자 생성
        var tempUser = new User
        {
            Id = 1,
            Name = "김신입",
            JoinDate = new DateTime(2026, 1, 2)
        };

        // OnboardingView에 DataContext 설정
        var onboardingView = new OnboardingView
        {
            DataContext = new OnboardingViewModel(tempUser)
        };

        // MainWindow의 콘텐츠로 설정
        this.Content = onboardingView;
    }
}
