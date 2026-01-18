using System.Windows.Controls;
using MyManual.Models;
using MyManual.Services.Interfaces;
using MyManual.ViewModels;
using MyManual.Views;

namespace MyManual.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IManualService _manualService;
        private readonly IOnboardingService _onboardingService;
        private ContentControl? _contentHost;

        // View/ViewModel 캐시
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
        private OnboardingManageView? _onboardingManageView;

        public NavigationService(IManualService manualService, IOnboardingService onboardingService)
        {
            _manualService = manualService;
            _onboardingService = onboardingService;
        }

        public void SetContentHost(ContentControl contentHost)
        {
            _contentHost = contentHost;
        }

        public void InitializeForUser(User user)
        {
            _onboardingViewModel = new OnboardingViewModel(user, _onboardingService);
            _onboardingView = new OnboardingView
            {
                DataContext = _onboardingViewModel
            };

            _onboardingViewModel.OpenManualRequested += manualId => NavigateToManualDetail(manualId);
        }

        public void NavigateToUserInit()
        {
            if (_userInitView == null)
            {
                _userInitViewModel = new UserInitViewModel();
                _userInitView = new UserInitView
                {
                    DataContext = _userInitViewModel
                };

                _userInitViewModel.UserRegistered += user =>
                {
                    App.SetCurrentUser(user);
                    InitializeForUser(user);
                    NavigateToOnboarding();
                };
            }

            SetContent(_userInitView);
        }

        public void NavigateToOnboarding()
        {
            SetContent(_onboardingView);
        }

        public void NavigateToCategoryMenu()
        {
            if (_categoryMenuView == null)
            {
                _categoryMenuViewModel = new CategoryMenuViewModel(_manualService);
                _categoryMenuView = new CategoryMenuView
                {
                    DataContext = _categoryMenuViewModel
                };

                _categoryMenuViewModel.CategoryClicked += category => NavigateToManualByCategory(category);
                _categoryMenuViewModel.ManualClicked += manualId => NavigateToManualDetail(manualId);
            }

            _categoryMenuViewModel?.Refresh();
            SetContent(_categoryMenuView);
        }

        public void NavigateToManual()
        {
            EnsureManualViewCreated();
            _manualViewModel?.Refresh();
            SetContent(_manualView);
        }

        public void NavigateToManualDetail(int manualId)
        {
            EnsureManualViewCreated();
            _manualViewModel?.Refresh();
            _manualViewModel?.NavigateToManual(manualId);
            SetContent(_manualView);
        }

        public void NavigateToManualByCategory(string category)
        {
            EnsureManualViewCreated();
            _manualViewModel?.Refresh();
            _manualViewModel?.FilterByCategory(category);
            SetContent(_manualView);
        }

        public void NavigateToManualCreate()
        {
            EnsureManualCreateViewCreated();
            _manualCreateViewModel?.Clear();
            SetContent(_manualCreateView);
        }

        public void NavigateToManualEdit(int manualId)
        {
            EnsureManualCreateViewCreated();
            _manualCreateViewModel?.LoadForEdit(manualId);
            SetContent(_manualCreateView);
        }

        private void EnsureManualViewCreated()
        {
            if (_manualView == null)
            {
                _manualViewModel = new ManualViewModel(_manualService);
                _manualView = new ManualView
                {
                    DataContext = _manualViewModel
                };
            }
        }

        private void EnsureManualCreateViewCreated()
        {
            if (_manualCreateView == null)
            {
                _manualCreateViewModel = new ManualCreateViewModel(_manualService);
                _manualCreateView = new ManualCreateView
                {
                    DataContext = _manualCreateViewModel
                };

                _manualCreateViewModel.SubmitRequested += () => NavigateToManual();
                _manualCreateViewModel.CancelRequested += () => NavigateToManual();
            }
        }

        public void NavigateToOnboardingManage()
        {
            if (_onboardingManageView == null)
            {
                _onboardingManageView = new OnboardingManageView();
                _onboardingManageView.NavigateToOnboarding += () => NavigateToOnboarding();
                _onboardingManageView.NavigateToManual += () => NavigateToManual();
                _onboardingManageView.NavigateToCategory += () => NavigateToCategoryMenu();
            }

            _onboardingManageView.Refresh();
            SetContent(_onboardingManageView);
        }

        private void SetContent(object? content)
        {
            if (_contentHost != null)
            {
                _contentHost.Content = content;
            }
        }
    }
}
