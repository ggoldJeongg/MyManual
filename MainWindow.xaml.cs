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

        // 사용자가 등록되어 있으면 해당 화면으로, 아니면 초기화면으로
        if (App.CurrentUser != null)
        {
            _navigationService.InitializeForUser(App.CurrentUser);

            // 관리자는 매뉴얼 화면으로, 일반 사용자는 온보딩 화면으로
            if (App.CurrentUser.IsAdmin)
            {
                _navigationService.NavigateToManual();
            }
            else
            {
                _navigationService.NavigateToOnboarding();
            }
        }
        else
        {
            _navigationService.NavigateToUserInit();
        }
    }
}
