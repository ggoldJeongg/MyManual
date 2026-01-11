using System.Windows;
using MyManual.Models.User;
using MyManual.ViewModels;
using MyManual.Views;

namespace MyManual;

public partial class MainWindow : Window
{
    private UserInitView? _userInitView;
    private UserInitViewModel? _userInitViewModel;
    private OnboardingView? _onboardingView;
    private OnboardingViewModel? _onboardingViewModel;
    private CategoryMenuView? _categoryMenuView;
    private CategoryMenuViewModel? _categoryMenuViewModel;
    private ManualView? _manualView;
    private ManualViewModel? _manualViewModel;
    private ManualCreateView? _manualCreateView;
    private ManualCreateViewModel? _manualCreateViewModel;

    public MainWindow()
    {
        InitializeComponent();

        // 사용자가 등록되어 있으면 온보딩으로, 아니면 초기화면으로
        if (App.CurrentUser != null)
        {
            InitializeOnboarding(App.CurrentUser);
            NavigateToOnboarding();
        }
        else
        {
            NavigateToUserInit();
        }
    }

    private void InitializeOnboarding(User user)
    {
        _onboardingViewModel = new OnboardingViewModel(user);
        _onboardingView = new OnboardingView
        {
            DataContext = _onboardingViewModel
        };

        // 이벤트 구독: 온보딩에서 매뉴얼 열기 요청 시
        _onboardingViewModel.OpenManualRequested += OnOpenManualRequested;
    }

    private void OnOpenManualRequested(int manualId)
    {
        NavigateToManualDetail(manualId);
    }

    // 초기 사용자 등록 화면으로 이동
    public void NavigateToUserInit()
    {
        if (_userInitView == null)
        {
            _userInitViewModel = new UserInitViewModel();
            _userInitView = new UserInitView
            {
                DataContext = _userInitViewModel
            };

            // 사용자 등록 완료 이벤트
            _userInitViewModel.UserRegistered += OnUserRegistered;
        }

        MainContent.Content = _userInitView;
    }

    private void OnUserRegistered(User user)
    {
        // 사용자 정보 저장 및 앱 전역 설정
        App.SetCurrentUser(user);

        // 온보딩 화면 초기화 및 이동
        InitializeOnboarding(user);
        NavigateToOnboarding();
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

    // 매뉴얼 생성 화면으로 이동
    public void NavigateToManualCreate()
    {
        if (_manualCreateView == null)
        {
            _manualCreateViewModel = new ManualCreateViewModel();
            _manualCreateView = new ManualCreateView
            {
                DataContext = _manualCreateViewModel
            };

            // 이벤트 구독
            _manualCreateViewModel.SubmitRequested += OnManualCreateSubmit;
            _manualCreateViewModel.CancelRequested += OnManualCreateCancel;
        }

        _manualCreateViewModel?.Clear();
        MainContent.Content = _manualCreateView;
    }

    private void OnManualCreateSubmit()
    {
        // TODO: DB 저장 후 매뉴얼 목록으로 이동
        NavigateToManual();
    }

    private void OnManualCreateCancel()
    {
        NavigateToManual();
    }
}
