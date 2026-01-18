using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MyManual.Models;
using MyManual.ViewModels;

namespace MyManual.Services
{
    public class DataService
    {
        private static DataService? _instance;
        public static DataService Instance => _instance ??= new DataService();

        private List<Manual>? _manuals;
        private Dictionary<int, List<OnboardingTaskJson>>? _onboardingTasks;

        private readonly string _dataPath;

        private DataService()
        {
            // 실행 파일 위치의 Data 폴더
            _dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        }

        // 모든 매뉴얼 가져오기
        public List<Manual> GetAllManuals()
        {
            if (_manuals == null)
            {
                LoadManuals();
            }
            return _manuals ?? new List<Manual>();
        }

        // ID로 매뉴얼 가져오기
        public Manual? GetManualById(int id)
        {
            var manuals = GetAllManuals();
            return manuals.Find(m => m.Id == id);
        }

        // 카테고리별 매뉴얼 가져오기
        public List<Manual> GetManualsByCategory(string category)
        {
            var manuals = GetAllManuals();
            return manuals.FindAll(m => m.Category == category);
        }

        // Day별 온보딩 태스크 가져오기 (ViewModel용)
        public List<OnboardingTaskViewModel> GetTasksForDay(int day)
        {
            if (_onboardingTasks == null)
            {
                LoadOnboardingTasks();
            }

            if (_onboardingTasks != null && _onboardingTasks.TryGetValue(day, out var tasks))
            {
                // 새 인스턴스로 반환 (상태 분리)
                return tasks.ConvertAll(t => new OnboardingTaskViewModel
                {
                    Id = t.Id,
                    Day = day,
                    Title = t.Title,
                    ManualId = t.ManualId,
                    IsCompleted = false
                });
            }

            // 기본값: 자율 업무
            return new List<OnboardingTaskViewModel>
            {
                new() { Id = 100 + day, Day = day, Title = $"Day {day} 자율 업무", ManualId = 900 }
            };
        }

        // 매뉴얼 JSON 로드
        private void LoadManuals()
        {
            try
            {
                var filePath = Path.Combine(_dataPath, "manuals.json");
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    _manuals = JsonSerializer.Deserialize<List<Manual>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"매뉴얼 로드 실패: {ex.Message}");
                _manuals = new List<Manual>();
            }
        }

        // 온보딩 태스크 JSON 로드
        private void LoadOnboardingTasks()
        {
            _onboardingTasks = new Dictionary<int, List<OnboardingTaskJson>>();

            try
            {
                var filePath = Path.Combine(_dataPath, "onboarding_tasks.json");
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var dayTasks = JsonSerializer.Deserialize<List<DayTasksJson>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (dayTasks != null)
                    {
                        foreach (var dt in dayTasks)
                        {
                            _onboardingTasks[dt.Day] = dt.Tasks;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"온보딩 태스크 로드 실패: {ex.Message}");
            }
        }

        // JSON 역직렬화용 내부 클래스
        private class DayTasksJson
        {
            public int Day { get; set; }
            public List<OnboardingTaskJson> Tasks { get; set; } = new();
        }

        private class OnboardingTaskJson
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public int ManualId { get; set; }
        }
    }
}
