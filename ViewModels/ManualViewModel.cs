using MyManual.Commands;
using MyManual.Models;
using MyManual.Services;
using MyManual.Services.Interfaces;
using MyManual.ViewModels.Base;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
                    LoadChecklistItems();
                    OnPropertyChanged(nameof(ChecklistProgress));
                    OnPropertyChanged(nameof(ChecklistProgressText));
                }
            }
        }

        public bool HasSelectedManual => SelectedManual != null;

        // 관리자 여부 (매뉴얼 입력 버튼 표시용)
        public bool IsAdmin => App.CurrentUser?.IsAdmin ?? false;

        // 체크리스트 항목 (사용자별 체크 상태 포함)
        private ObservableCollection<ChecklistItemViewModel> _checklistItems = new();
        public ObservableCollection<ChecklistItemViewModel> ChecklistItems
        {
            get => _checklistItems;
            set => SetProperty(ref _checklistItems, value);
        }

        // 체크리스트 진행률
        public double ChecklistProgress
        {
            get
            {
                if (SelectedManual?.Checklist == null || SelectedManual.Checklist.Count == 0)
                    return 0;

                var userId = App.CurrentUser?.Id ?? 0;
                if (userId == 0) return 0;

                var (completed, total) = _manualService.GetChecklistProgress(userId, SelectedManual.Id);
                return total > 0 ? (double)completed / total * 100 : 0;
            }
        }

        public string ChecklistProgressText
        {
            get
            {
                if (SelectedManual?.Checklist == null || SelectedManual.Checklist.Count == 0)
                    return "0/0 완료";

                var userId = App.CurrentUser?.Id ?? 0;
                if (userId == 0) return "0/0 완료";

                var (completed, total) = _manualService.GetChecklistProgress(userId, SelectedManual.Id);
                var progress = total > 0 ? (double)completed / total * 100 : 0;
                return $"{completed}/{total} 완료 ({progress:F0}%)";
            }
        }

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

        // ==================== Commands ====================

        public ICommand SelectManualCommand { get; }
        public ICommand ToggleChecklistCommand { get; }
        public ICommand ClearFilterCommand { get; }

        // ==================== 생성자 ====================

        public ManualViewModel(IManualService manualService)
        {
            // Service 주입
            _manualService = manualService;

            // 데이터 로드
            LoadManuals();

            // Command 초기화
            SelectManualCommand = new RelayCommand(OnSelectManual);
            ToggleChecklistCommand = new RelayCommand(OnToggleChecklist);
            ClearFilterCommand = new RelayCommand(OnClearFilter);
        }

        // ==================== 메서드 ====================

        private void LoadManuals()
        {
            _allManuals = _manualService.GetAllManuals();

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

            // 매뉴얼 목록 표시
            FilterManuals();

            // 첫 번째 매뉴얼 선택
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

        private void OnToggleChecklist(object? parameter)
        {
            if (parameter is ChecklistItemViewModel item)
            {
                var userId = App.CurrentUser?.Id ?? 0;
                if (userId == 0) return;

                // DB에 체크 상태 저장
                _manualService.SetChecklistStatus(userId, item.Id, item.IsChecked);
            }

            // 진행률 업데이트
            OnPropertyChanged(nameof(ChecklistProgress));
            OnPropertyChanged(nameof(ChecklistProgressText));
        }

        // 선택된 매뉴얼의 체크리스트 항목 로드 (사용자별 체크 상태 포함)
        private void LoadChecklistItems()
        {
            if (SelectedManual == null)
            {
                ChecklistItems = new ObservableCollection<ChecklistItemViewModel>();
                return;
            }

            var userId = App.CurrentUser?.Id ?? 0;
            var statuses = userId > 0
                ? _manualService.GetUserChecklistStatuses(userId, SelectedManual.Id)
                    .ToDictionary(s => s.ChecklistItemId, s => s.IsChecked)
                : new Dictionary<int, bool>();

            var items = SelectedManual.Checklist
                .OrderBy(c => c.OrderIndex)
                .Select(c => new ChecklistItemViewModel
                {
                    Id = c.Id,
                    ManualId = c.ManualId,
                    Content = c.Content,
                    OrderIndex = c.OrderIndex,
                    IsChecked = statuses.TryGetValue(c.Id, out var isChecked) && isChecked
                })
                .ToList();

            ChecklistItems = new ObservableCollection<ChecklistItemViewModel>(items);
        }

        private void OnClearFilter(object? parameter)
        {
            SearchText = string.Empty;
            SelectedCategory = "전체";
        }

        // 특정 매뉴얼 ID로 이동 (OnboardingView에서 호출용)
        public void NavigateToManual(int manualId)
        {
            var manual = _allManuals.Find(m => m.Id == manualId);
            if (manual != null)
            {
                // 필터 초기화
                SearchText = string.Empty;
                SelectedCategory = "전체";
                FilterManuals();

                // 해당 매뉴얼 선택
                SelectedManual = manual;
            }
        }

        // 카테고리로 필터링 (CategoryMenuView에서 호출용)
        public void FilterByCategory(string category)
        {
            SearchText = string.Empty;
            SelectedCategory = category;
            FilterManuals();

            // 첫 번째 매뉴얼 선택
            if (Manuals.Count > 0)
            {
                SelectedManual = Manuals[0];
            }
        }

        // 매뉴얼 목록 새로고침 (매뉴얼 생성 후 호출용)
        public void Refresh()
        {
            LoadManuals();
        }
    }
}
