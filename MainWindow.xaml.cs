using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MyManual.Services;
using MyManual.Services.Interfaces;

namespace MyManual;

public partial class MainWindow : Window
{
    private readonly NavigationService _navigationService;

    public MainWindow()
    {
        InitializeComponent();

        // NavigationService 가져오기 및 ContentHost 설정
        _navigationService = (NavigationService)App.Services.GetRequiredService<INavigationService>();
        _navigationService.SetContentHost(MainContent);

        // 사용자가 등록되어 있으면 온보딩으로, 아니면 초기화면으로
        if (App.CurrentUser != null)
        {
            _navigationService.InitializeForUser(App.CurrentUser);
            _navigationService.NavigateToOnboarding();
        }
        else
        {
            _navigationService.NavigateToUserInit();
        }
    }
}
