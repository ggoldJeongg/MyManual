using System.Windows;
using MyManual.ViewModels;
using MyManual.Views;

namespace MyManual;

public partial class MainWindow : Window
{
    private OnboardingView? _onboardingView;
    private OnboardingViewModel? _onboardingViewModel;
    private ManualView? _manualView;
    private ManualViewModel? _manualViewModel;

    public MainWindow()
    {
        InitializeComponent();

        // App.xaml.cs에서 설정한 CurrentUser 사용
        if (App.CurrentUser != null)
        {
            _onboardingViewModel = new OnboardingViewModel(App.CurrentUser);
            _onboardingView = new OnboardingView
            {
                DataContext = _onboardingViewModel
            };

            // 이벤트 구독: 온보딩에서 매뉴얼 열기 요청 시
            _onboardingViewModel.OpenManualRequested += OnOpenManualRequested;

            // 초기 화면: 온보딩
            NavigateToOnboarding();
        }
    }

    private void OnOpenManualRequested(int manualId)
    {
        NavigateToManual(manualId);
    }

    // 온보딩 화면으로 이동
    public void NavigateToOnboarding()
    {
        MainContent.Content = _onboardingView;
    }

    // 매뉴얼 화면으로 이동
    public void NavigateToManual(int? manualId = null)
    {
        // ManualView가 없으면 생성
        if (_manualView == null)
        {
            _manualViewModel = new ManualViewModel();
            _manualView = new ManualView
            {
                DataContext = _manualViewModel
            };
        }

        // 특정 매뉴얼로 이동
        if (manualId.HasValue && _manualViewModel != null)
        {
            _manualViewModel.NavigateToManual(manualId.Value);
        }

        MainContent.Content = _manualView;
    }
}
