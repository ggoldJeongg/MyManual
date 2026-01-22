using MyManual.Commands;
using MyManual.Exceptions;
using MyManual.Models;
using MyManual.Services;
using MyManual.Services.Interfaces;
using MyManual.ViewModels.Base;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyManual.ViewModels
{
    public class ManualViewModel : ViewModelBase
    {
        // ==================== Service ====================

        private readonly IManualService _manualService;

        // ==================== 데이터 ====================

        private ObservableCollection<Manual> _manuals = new();
        public ObservableCollection<Manual> Manuals
        {
            get => _manuals;
            set => SetProperty(ref _manuals, value);
        }

        private Manual? _selectedManual;
        public Manual? SelectedManual
        {
            get => _selectedManual;
            set
            {
                if (SetProperty(ref _selectedManual, value))
                {
                    OnPropertyChanged(nameof(HasSelectedManual));
                }
            }
        }

        public bool HasSelectedManual => SelectedManual != null;

        // 관리자 여부 (매뉴얼 입력 버튼 표시용)
        public bool IsAdmin => App.CurrentUser?.IsAdmin ?? false;

        // ==================== 카테고리 필터링 ====================

        private ObservableCollection<string> _categories = new();
        public ObservableCollection<string> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        private string? _selectedCategory;
        public string? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    FilterManuals();
                }
            }
        }

        // ==================== 검색 ====================

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterManuals();
                }
            }
        }

        // 전체 매뉴얼 (필터링 전)
        private List<Manual> _allManuals = new();

        // 로딩 중복 방지
        private bool _isLoading = false;

        // 로딩 완료 후 선택할 매뉴얼 ID (NavigateToManual 대기용)
        private int? _pendingManualId = null;

        // 로딩 완료 후 적용할 카테고리 (FilterByCategory 대기용)
        private string? _pendingCategory = null;

        // ==================== Commands ====================

        public ICommand SelectManualCommand { get; }
        public ICommand ClearFilterCommand { get; }

        // ==================== 생성자 ====================

        public ManualViewModel(IManualService manualService)
        {
            // Service 주입
            _manualService = manualService;

            // 데이터 로드 (비동기)
            _ = LoadManualsAsync();

            // Command 초기화
            SelectManualCommand = new RelayCommand(OnSelectManual);
            ClearFilterCommand = new RelayCommand(OnClearFilter);
        }

        // ==================== 메서드 ====================

        private async Task LoadManualsAsync()
        {
            // 중복 로딩 방지
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                _allManuals = await _manualService.GetAllManualsAsync();

                // 카테고리 목록 추출
                var categorySet = new HashSet<string> { "전체" };
                foreach (var manual in _allManuals)
                {
                    if (!string.IsNullOrEmpty(manual.Category))
                    {
                        categorySet.Add(manual.Category);
                    }
                }
                Categories = new ObservableCollection<string>(categorySet);
                _selectedCategory = "전체";
                OnPropertyChanged(nameof(SelectedCategory));

                // 매뉴얼 목록 표시
                FilterManuals();

                // 대기 중인 카테고리 필터 적용
                if (_pendingCategory != null)
                {
                    var category = _pendingCategory;
                    _pendingCategory = null;
                    ApplyPendingCategory(category);
                }
                // 대기 중인 매뉴얼 ID 선택
                else if (_pendingManualId != null)
                {
                    var manualId = _pendingManualId.Value;
                    _pendingManualId = null;
                    ApplyPendingManualId(manualId);
                }
                // 기본: 첫 번째 매뉴얼 선택
                else if (Manuals.Count > 0)
                {
                    SelectedManual = Manuals[0];
                }
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.Handle(ex, "매뉴얼 목록 로드");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ApplyPendingManualId(int manualId)
        {
            var manual = _allManuals.Find(m => m.Id == manualId);
            if (manual != null)
            {
                SearchText = string.Empty;
                _selectedCategory = "전체";
                OnPropertyChanged(nameof(SelectedCategory));
                FilterManuals();
                SelectedManual = manual;
            }
            else if (Manuals.Count > 0)
            {
                SelectedManual = Manuals[0];
            }
        }

        private void ApplyPendingCategory(string category)
        {
            SearchText = string.Empty;
            _selectedCategory = category;
            OnPropertyChanged(nameof(SelectedCategory));
            FilterManuals();

            if (Manuals.Count > 0)
            {
                SelectedManual = Manuals[0];
            }
        }

        private void FilterManuals()
        {
            var filtered = _allManuals.AsEnumerable();

            // 카테고리 필터
            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "전체")
            {
                filtered = filtered.Where(m => m.Category == SelectedCategory);
            }

            // 검색어 필터
            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(m =>
                    m.Title.ToLower().Contains(searchLower) ||
                    m.Purpose.ToLower().Contains(searchLower) ||
                    m.Category.ToLower().Contains(searchLower));
            }

            Manuals = new ObservableCollection<Manual>(filtered);

            // 선택된 매뉴얼이 필터링된 목록에 없으면 첫 번째 선택
            if (SelectedManual != null && !Manuals.Contains(SelectedManual))
            {
                SelectedManual = Manuals.FirstOrDefault();
            }
        }

        private void OnSelectManual(object? parameter)
        {
            if (parameter is Manual manual)
            {
                SelectedManual = manual;
            }
        }

        private void OnClearFilter(object? parameter)
        {
            SearchText = string.Empty;
            SelectedCategory = "전체";
        }

        // 특정 매뉴얼 ID로 이동 (OnboardingView에서 호출용)
        public void NavigateToManual(int manualId)
        {
            // 로딩 중이면 대기열에 추가
            if (_isLoading || _allManuals.Count == 0)
            {
                _pendingManualId = manualId;
                _pendingCategory = null;
                return;
            }

            ApplyPendingManualId(manualId);
        }

        // 카테고리로 필터링 (CategoryMenuView에서 호출용)
        public void FilterByCategory(string category)
        {
            // 로딩 중이면 대기열에 추가
            if (_isLoading || _allManuals.Count == 0)
            {
                _pendingCategory = category;
                _pendingManualId = null;
                return;
            }

            ApplyPendingCategory(category);
        }

        // 매뉴얼 목록 새로고침 (매뉴얼 생성 후 호출용)
        public void Refresh()
        {
            // 사용자 권한 변경 알림 (다른 사용자 로그인 시)
            OnPropertyChanged(nameof(IsAdmin));
            _ = LoadManualsAsync();
        }

        // 선택된 매뉴얼 삭제 (비동기)
        public async Task DeleteSelectedManualAsync()
        {
            if (SelectedManual == null) return;

            var currentUser = App.CurrentUser;
            if (currentUser == null || !currentUser.IsAdmin) return;

            try
            {
                var manualId = SelectedManual.Id;
                var deleted = await _manualService.DeleteManualAsync(manualId, currentUser);

                if (deleted)
                {
                    // 목록에서 제거
                    _allManuals.RemoveAll(m => m.Id == manualId);
                    Manuals.Remove(SelectedManual);

                    // 다음 매뉴얼 선택
                    SelectedManual = Manuals.FirstOrDefault();

                    System.Diagnostics.Debug.WriteLine($"[매뉴얼 삭제] ID: {manualId}");
                }
            }
            catch (System.Exception ex)
            {
                ExceptionHandler.Handle(ex, "매뉴얼 삭제");
            }
        }
    }
}
