using System.Windows;
using MyManual.ViewModels;
using MyManual.Views;

namespace MyManual;

public partial class MainWindow : Window
{
    private OnboardingView? _onboardingView;
    private OnboardingViewModel? _onboardingViewModel;
    private CategoryMenuView? _categoryMenuView;
    private CategoryMenuViewModel? _categoryMenuViewModel;
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
        NavigateToManualDetail(manualId);
    }

    // 온보딩 화면으로 이동
    public void NavigateToOnboarding()
    {
        MainContent.Content = _onboardingView;
    }

    // 카테고리 메뉴 화면으로 이동
    public void NavigateToCategoryMenu()
    {
        // CategoryMenuView가 없으면 생성
        if (_categoryMenuView == null)
        {
            _categoryMenuViewModel = new CategoryMenuViewModel();
            _categoryMenuView = new CategoryMenuView
            {
                DataContext = _categoryMenuViewModel
            };

            // 이벤트 구독
            _categoryMenuViewModel.CategoryClicked += OnCategoryClicked;
            _categoryMenuViewModel.ManualClicked += OnManualClicked;
        }

        MainContent.Content = _categoryMenuView;
    }

    // 카테고리 클릭 시 -> ManualView에서 해당 카테고리 필터링
    private void OnCategoryClicked(string category)
    {
        NavigateToManualByCategory(category);
    }

    // 매뉴얼 클릭 시 -> ManualView에서 해당 매뉴얼 선택
    private void OnManualClicked(int manualId)
    {
        NavigateToManualDetail(manualId);
    }

    // 매뉴얼 상세 화면으로 이동 (특정 매뉴얼 ID)
    public void NavigateToManualDetail(int manualId)
    {
        EnsureManualViewCreated();
        _manualViewModel?.NavigateToManual(manualId);
        MainContent.Content = _manualView;
    }

    // 매뉴얼 화면으로 이동 (카테고리 필터링)
    public void NavigateToManualByCategory(string category)
    {
        EnsureManualViewCreated();
        _manualViewModel?.FilterByCategory(category);
        MainContent.Content = _manualView;
    }

    // 매뉴얼 화면으로 이동 (필터 없음)
    public void NavigateToManual()
    {
        EnsureManualViewCreated();
        MainContent.Content = _manualView;
    }

    // ManualView 생성 보장
    private void EnsureManualViewCreated()
    {
        if (_manualView == null)
        {
            _manualViewModel = new ManualViewModel();
            _manualView = new ManualView
            {
                DataContext = _manualViewModel
            };
        }
    }
}
