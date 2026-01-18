using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MyManual.Data;
using MyManual.Models;

namespace MyManual.Services
{
    /// <summary>
    /// DB 초기화 및 JSON 데이터 마이그레이션
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly AppDbContext _db;
        private readonly string _jsonDataPath;

        // JSON Id → DB Id 매핑 (Manual용)
        private Dictionary<int, int> _manualIdMap = new();

        public DatabaseInitializer(AppDbContext db)
        {
            _db = db;
            _jsonDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        }

        /// <summary>
        /// DB 초기화 (테이블 생성 + 초기 데이터 마이그레이션)
        /// </summary>
        public void Initialize()
        {
            // 1. 테이블 생성 (없으면 생성)
            _db.Database.EnsureCreated();

            // 2. DELETE 모드 사용 (NAS/네트워크 공유 폴더 호환)
            _db.Database.ExecuteSqlRaw("PRAGMA journal_mode=DELETE;");
            _db.Database.ExecuteSqlRaw("PRAGMA busy_timeout=5000;");
            System.Diagnostics.Debug.WriteLine("[DB 초기화] DELETE 모드 활성화");

            // 3. 스키마 업데이트 (새 컬럼 추가)
            UpdateSchema();

            // 3. 데이터가 비어있으면 JSON에서 마이그레이션
            bool migratedManuals = false;
            bool migratedTasks = false;

            if (!_db.Manuals.Any())
            {
                MigrateManuals();
                migratedManuals = true;
            }
            else
            {
                // 이미 매뉴얼이 있으면 매핑 빌드 (Title로 매칭)
                BuildManualIdMapFromExisting();
            }

            if (!_db.OnboardingTasks.Any())
            {
                MigrateOnboardingTasks();
                migratedTasks = true;
            }

            // 디버그용 로그
            var manualCount = _db.Manuals.Count();
            var taskCount = _db.OnboardingTasks.Count();
            System.Diagnostics.Debug.WriteLine($"[DB 초기화] 매뉴얼: {manualCount}개, 태스크: {taskCount}개");
            System.Diagnostics.Debug.WriteLine($"[DB 초기화] 마이그레이션 실행: Manuals={migratedManuals}, Tasks={migratedTasks}");
        }

        /// <summary>
        /// 스키마 업데이트 (새 컬럼 추가)
        /// </summary>
        private void UpdateSchema()
        {
            try
            {
                // OnboardingTasks 테이블에 UserId, OrderIndex, CreatedAt 컬럼 추가
                AddColumnIfNotExists("OnboardingTasks", "UserId", "INTEGER");
                AddColumnIfNotExists("OnboardingTasks", "OrderIndex", "INTEGER DEFAULT 0");
                AddColumnIfNotExists("OnboardingTasks", "CreatedAt", "TEXT DEFAULT CURRENT_TIMESTAMP");

                // Users 테이블에 PasswordHash 컬럼 추가
                AddColumnIfNotExists("Users", "PasswordHash", "TEXT DEFAULT ''");

                System.Diagnostics.Debug.WriteLine("[스키마 업데이트] 완료");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[스키마 업데이트] 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 테이블에 컬럼이 없으면 추가
        /// </summary>
        /// <remarks>
        /// tableName, columnName, columnType은 코드에서 하드코딩된 값만 사용되므로
        /// SQL Injection 위험이 없습니다. DDL 문에서는 파라미터 바인딩을 사용할 수 없으므로
        /// ExecuteSqlRaw를 사용합니다.
        /// </remarks>
        private void AddColumnIfNotExists(string tableName, string columnName, string columnType)
        {
            // 허용된 테이블명/컬럼명 검증 (화이트리스트)
            var allowedTables = new[] { "OnboardingTasks", "Users" };
            var allowedColumns = new[] { "UserId", "OrderIndex", "CreatedAt", "PasswordHash" };

            if (!allowedTables.Contains(tableName) || !allowedColumns.Contains(columnName))
            {
                System.Diagnostics.Debug.WriteLine($"[스키마] 허용되지 않은 테이블/컬럼: {tableName}.{columnName}");
                return;
            }

            try
            {
                // 테이블 정보 조회
                var connection = _db.Database.GetDbConnection();
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info({tableName})";

                var columnExists = false;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader.GetString(1);
                        if (name.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                        {
                            columnExists = true;
                            break;
                        }
                    }
                }

                if (!columnExists)
                {
                    // 화이트리스트로 검증된 값만 사용하므로 SQL Injection 안전
#pragma warning disable EF1002
                    _db.Database.ExecuteSqlRaw($"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnType}");
#pragma warning restore EF1002
                    System.Diagnostics.Debug.WriteLine($"[스키마] {tableName}.{columnName} 컬럼 추가됨");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[스키마] {tableName}.{columnName} 추가 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 이미 DB에 매뉴얼이 있을 때, Title 기반으로 JSON Id → DB Id 매핑 빌드
        /// </summary>
        private void BuildManualIdMapFromExisting()
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

                var dbManuals = _db.Manuals.ToList();

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
        /// manuals.json → DB 마이그레이션 (트랜잭션 적용)
        /// </summary>
        private void MigrateManuals()
        {
            var filePath = Path.Combine(_jsonDataPath, "manuals.json");
            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine("manuals.json 파일을 찾을 수 없습니다.");
                return;
            }

            using IDbContextTransaction transaction = _db.Database.BeginTransaction();
            try
            {
                var json = File.ReadAllText(filePath);
                var jsonManuals = JsonSerializer.Deserialize<List<ManualJson>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (jsonManuals == null)
                {
                    transaction.Rollback();
                    return;
                }

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

                    _db.Manuals.Add(manual);
                    _db.SaveChanges(); // 각 매뉴얼 저장 후 ID 확정

                    // JSON Id → DB Id 매핑 저장
                    _manualIdMap[jm.Id] = manual.Id;
                    System.Diagnostics.Debug.WriteLine($"[매핑] JSON Id {jm.Id} → DB Id {manual.Id} ({jm.Title})");
                }

                transaction.Commit();
                System.Diagnostics.Debug.WriteLine($"매뉴얼 {jsonManuals.Count}개 마이그레이션 완료");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _manualIdMap.Clear(); // 롤백 시 매핑도 초기화
                System.Diagnostics.Debug.WriteLine($"매뉴얼 마이그레이션 실패 (롤백됨): {ex.Message}");
            }
        }

        /// <summary>
        /// onboarding_tasks.json → DB 마이그레이션 (트랜잭션 적용)
        /// </summary>
        private void MigrateOnboardingTasks()
        {
            var filePath = Path.Combine(_jsonDataPath, "onboarding_tasks.json");
            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine("onboarding_tasks.json 파일을 찾을 수 없습니다.");
                return;
            }

            using IDbContextTransaction transaction = _db.Database.BeginTransaction();
            try
            {
                var json = File.ReadAllText(filePath);
                var dayTasks = JsonSerializer.Deserialize<List<DayTasksJson>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (dayTasks == null)
                {
                    transaction.Rollback();
                    return;
                }

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

                        _db.OnboardingTasks.Add(task);
                        count++;
                    }
                }

                _db.SaveChanges();
                transaction.Commit();
                System.Diagnostics.Debug.WriteLine($"온보딩 태스크 {count}개 마이그레이션 완료 (건너뜀: {skipped}개)");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                System.Diagnostics.Debug.WriteLine($"온보딩 태스크 마이그레이션 실패 (롤백됨): {ex.Message}");
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
