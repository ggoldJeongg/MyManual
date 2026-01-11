using System.Windows;
using System.Windows.Controls;
using MyManual.ViewModels;
using MyManual.Views;

namespace MyManual;

public partial class MainWindow : Window
{
    private OnboardingView? _onboardingView;
    private ManualView? _manualView;
    private OnboardingViewModel? _onboardingViewModel;
    private ManualViewModel? _manualViewModel;

    public MainWindow()
    {
        InitializeComponent();

        // ViewModel 초기화
        if (App.CurrentUser != null)
        {
            _onboardingViewModel = new OnboardingViewModel(App.CurrentUser);
            _manualViewModel = new ManualViewModel();

            // 이벤트 구독: 온보딩에서 매뉴얼 열기 요청 시
            _onboardingViewModel.OpenManualRequested += OnOpenManualRequested;

            // View 초기화
            _onboardingView = new OnboardingView { DataContext = _onboardingViewModel };
            _manualView = new ManualView { DataContext = _manualViewModel };

            // 시작 화면: 온보딩
            NavigateToOnboarding();
        }
    }

    // 온보딩에서 매뉴얼 열기 요청 시 호출
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
        MainContent.Content = _manualView;

        // 특정 매뉴얼로 이동 요청 시
        if (manualId.HasValue && _manualViewModel != null)
        {
            _manualViewModel.NavigateToManual(manualId.Value);
        }
    }
}
