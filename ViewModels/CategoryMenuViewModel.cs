using MyManual.Models;
using MyManual.Services;
using MyManual.Services.Interfaces;
using MyManual.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyManual.ViewModels
{
    public class CategoryMenuViewModel : ViewModelBase
    {
        // ==================== Service ====================

        private readonly IManualService _manualService;

        // 카테고리별 매뉴얼 그룹
        private ObservableCollection<CategoryGroup> _categoryGroups = new();
        public ObservableCollection<CategoryGroup> CategoryGroups
        {
            get => _categoryGroups;
            set => SetProperty(ref _categoryGroups, value);
        }

        // 카테고리 클릭 이벤트 (MainWindow에서 구독)
        public event Action<string>? CategoryClicked;

        // 매뉴얼 클릭 이벤트 (MainWindow에서 구독)
        public event Action<int>? ManualClicked;

        public CategoryMenuViewModel(IManualService manualService)
        {
            _manualService = manualService;
            LoadCategoryGroups();
        }

        private void LoadCategoryGroups()
        {
            var manuals = _manualService.GetAllManuals();

            // 카테고리별로 그룹화하고, 각 그룹 내에서 최신순(Id 역순) 정렬
            var groups = manuals
                .GroupBy(m => m.Category)
                .Select(g => new CategoryGroup
                {
                    CategoryName = g.Key,
                    Manuals = new ObservableCollection<ManualSummary>(
                        g.OrderByDescending(m => m.Id) // 최신순 정렬 (Id가 클수록 최신)
                         .Select(m => new ManualSummary
                         {
                             Id = m.Id,
                             Title = m.Title
                         })
                    )
                })
                .OrderBy(g => GetCategoryOrder(g.CategoryName)) // 카테고리 순서 정렬
                .ToList();

            CategoryGroups = new ObservableCollection<CategoryGroup>(groups);
        }

        // 카테고리 표시 순서 정의
        private int GetCategoryOrder(string category)
        {
            return category switch
            {
                "사내 규칙" => 1,
                "업무도구 / 시스템" => 2,
                "실무 프로세스" => 3,
                "복지 / 생활가이드" => 4,
                _ => 99
            };
        }

        // 카테고리 클릭 처리
        public void OnCategoryClick(string category)
        {
            CategoryClicked?.Invoke(category);
        }

        // 매뉴얼 클릭 처리
        public void OnManualClick(int manualId)
        {
            ManualClicked?.Invoke(manualId);
        }

        // 카테고리 목록 새로고침
        public void Refresh()
        {
            LoadCategoryGroups();
        }
    }

    // 카테고리 그룹 모델
    public class CategoryGroup
    {
        public string CategoryName { get; set; } = string.Empty;
        public ObservableCollection<ManualSummary> Manuals { get; set; } = new();
    }

    // 매뉴얼 요약 모델
    public class ManualSummary
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
