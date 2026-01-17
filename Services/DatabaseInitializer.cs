using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyManual.Data;
using MyManual.Models;

namespace MyManual.Services
{
    /// <summary>
    /// DB 초기화 및 JSON 데이터 마이그레이션
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly string _jsonDataPath;

        // JSON Id → DB Id 매핑 (Manual용)
        private Dictionary<int, int> _manualIdMap = new();

        public DatabaseInitializer()
        {
            _jsonDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        }

        /// <summary>
        /// DB 초기화 (테이블 생성 + 초기 데이터 마이그레이션)
        /// </summary>
        public void Initialize()
        {
            var db = AppDbContext.Instance;

            // 1. 테이블 생성 (없으면 생성)
            db.Database.EnsureCreated();

            // 2. WAL 모드 활성화 (동시 접근 시 lock 방지)
            db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
            db.Database.ExecuteSqlRaw("PRAGMA busy_timeout=5000;");
            System.Diagnostics.Debug.WriteLine("[DB 초기화] WAL 모드 활성화");

            // 3. 데이터가 비어있으면 JSON에서 마이그레이션
            bool migratedManuals = false;
            bool migratedTasks = false;

            if (!db.Manuals.Any())
            {
                MigrateManuals(db);
                migratedManuals = true;
            }
            else
            {
                // 이미 매뉴얼이 있으면 매핑 빌드 (Title로 매칭)
                BuildManualIdMapFromExisting(db);
            }

            if (!db.OnboardingTasks.Any())
            {
                MigrateOnboardingTasks(db);
                migratedTasks = true;
            }

            // 디버그용 로그
            var manualCount = db.Manuals.Count();
            var taskCount = db.OnboardingTasks.Count();
            System.Diagnostics.Debug.WriteLine($"[DB 초기화] 매뉴얼: {manualCount}개, 태스크: {taskCount}개");
            System.Diagnostics.Debug.WriteLine($"[DB 초기화] 마이그레이션 실행: Manuals={migratedManuals}, Tasks={migratedTasks}");
        }

        /// <summary>
        /// 이미 DB에 매뉴얼이 있을 때, Title 기반으로 JSON Id → DB Id 매핑 빌드
        /// </summary>
        private void BuildManualIdMapFromExisting(AppDbContext db)
        {
            var filePath = Path.Combine(_jsonDataPath, "manuals.json");
            if (!File.Exists(filePath)) return;

            try
            {
                var json = File.ReadAllText(filePath);
                var jsonManuals = JsonSerializer.Deserialize<List<ManualJson>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (jsonManuals == null) return;

                var dbManuals = db.Manuals.ToList();

                foreach (var jm in jsonManuals)
                {
                    var dbManual = dbManuals.FirstOrDefault(m => m.Title == jm.Title);
                    if (dbManual != null)
                    {
                        _manualIdMap[jm.Id] = dbManual.Id;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[ID 매핑] 기존 매뉴얼에서 {_manualIdMap.Count}개 매핑 생성");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ID 매핑] 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// manuals.json → DB 마이그레이션
        /// </summary>
        private void MigrateManuals(AppDbContext db)
        {
            var filePath = Path.Combine(_jsonDataPath, "manuals.json");
            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine("manuals.json 파일을 찾을 수 없습니다.");
                return;
            }

            try
            {
                var json = File.ReadAllText(filePath);
                var jsonManuals = JsonSerializer.Deserialize<List<ManualJson>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (jsonManuals == null) return;

                foreach (var jm in jsonManuals)
                {
                    var manual = new Manual
                    {
                        // Id는 지정하지 않음 - DB가 자동 생성
                        Title = jm.Title,
                        Category = jm.Category,
                        Purpose = jm.Purpose,
                        Process = jm.Process,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    // 체크리스트 추가
                    if (jm.Checklist != null)
                    {
                        int orderIndex = 0;
                        foreach (var jc in jm.Checklist)
                        {
                            manual.Checklist.Add(new ChecklistItem
                            {
                                Content = jc.Content,
                                OrderIndex = orderIndex++
                            });
                        }
                    }

                    // 히스토리 추가
                    if (jm.History != null)
                    {
                        foreach (var jh in jm.History)
                        {
                            manual.History.Add(new HistoryItem
                            {
                                Date = jh.Date,
                                Description = jh.Description
                            });
                        }
                    }

                    db.Manuals.Add(manual);
                    db.SaveChanges(); // 각 매뉴얼 저장 후 ID 확정

                    // JSON Id → DB Id 매핑 저장
                    _manualIdMap[jm.Id] = manual.Id;
                    System.Diagnostics.Debug.WriteLine($"[매핑] JSON Id {jm.Id} → DB Id {manual.Id} ({jm.Title})");
                }

                System.Diagnostics.Debug.WriteLine($"매뉴얼 {jsonManuals.Count}개 마이그레이션 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"매뉴얼 마이그레이션 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// onboarding_tasks.json → DB 마이그레이션
        /// </summary>
        private void MigrateOnboardingTasks(AppDbContext db)
        {
            var filePath = Path.Combine(_jsonDataPath, "onboarding_tasks.json");
            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine("onboarding_tasks.json 파일을 찾을 수 없습니다.");
                return;
            }

            try
            {
                var json = File.ReadAllText(filePath);
                var dayTasks = JsonSerializer.Deserialize<List<DayTasksJson>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (dayTasks == null) return;

                int count = 0;
                int skipped = 0;

                foreach (var dt in dayTasks)
                {
                    foreach (var jt in dt.Tasks)
                    {
                        // JSON ManualId → DB ManualId로 변환
                        if (!_manualIdMap.TryGetValue(jt.ManualId, out var dbManualId))
                        {
                            System.Diagnostics.Debug.WriteLine($"[경고] ManualId {jt.ManualId} 매핑 없음, 태스크 '{jt.Title}' 건너뜀");
                            skipped++;
                            continue;
                        }

                        var task = new OnboardingTask
                        {
                            // Id는 지정하지 않음 - DB가 자동 생성
                            Day = dt.Day,
                            Title = jt.Title,
                            ManualId = dbManualId
                        };

                        db.OnboardingTasks.Add(task);
                        count++;
                    }
                }

                db.SaveChanges();
                System.Diagnostics.Debug.WriteLine($"온보딩 태스크 {count}개 마이그레이션 완료 (건너뜀: {skipped}개)");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"온보딩 태스크 마이그레이션 실패: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"상세: {ex}");
            }
        }

        // ==================== JSON 역직렬화용 클래스 ====================

        private class ManualJson
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string Purpose { get; set; } = string.Empty;
            public string Process { get; set; } = string.Empty;
            public List<ChecklistJson>? Checklist { get; set; }
            public List<HistoryJson>? History { get; set; }
        }

        private class ChecklistJson
        {
            public int Id { get; set; }
            public string Content { get; set; } = string.Empty;
            public bool IsChecked { get; set; }
        }

        private class HistoryJson
        {
            public string Date { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        private class DayTasksJson
        {
            public int Day { get; set; }
            public List<TaskJson> Tasks { get; set; } = new();
        }

        private class TaskJson
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public int ManualId { get; set; }
        }
    }
}
