using Microsoft.EntityFrameworkCore;
using MyManual.Data;
using MyManual.Models;
using MyManual.Services.Interfaces;
using MyManual.ViewModels;

namespace MyManual.Services
{
    /// <summary>
    /// 온보딩 태스크 관련 비즈니스 로직 + DB 접근
    /// </summary>
    public class OnboardingService : IOnboardingService
    {
        private readonly AppDbContext _db;

        public OnboardingService(AppDbContext db)
        {
            _db = db;
        }

        public List<OnboardingTaskViewModel> GetTasksForDay(int day, int userId)
        {
            // 해당 사용자의 해당 Day 태스크 조회
            var tasks = _db.OnboardingTasks
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.Day == day)
                .OrderBy(t => t.OrderIndex)
                .ThenBy(t => t.Id)
                .ToList();

            // 사용자 완료 상태 조회
            var taskIds = tasks.Select(t => t.Id).ToList();
            var statuses = _db.UserTaskStatuses
                .AsNoTracking()
                .Where(s => s.UserId == userId && taskIds.Contains(s.OnboardingTaskId))
                .ToDictionary(s => s.OnboardingTaskId, s => s.IsCompleted);

            // ViewModel로 변환
            return tasks.Select(t => new OnboardingTaskViewModel
            {
                Id = t.Id,
                Day = t.Day,
                Title = t.Title,
                ManualId = t.ManualId,
                IsCompleted = statuses.TryGetValue(t.Id, out var completed) && completed
            }).ToList();
        }

        public List<OnboardingTask> GetUserTasks(int userId)
        {
            return _db.OnboardingTasks
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Day)
                .ThenBy(t => t.OrderIndex)
                .ThenBy(t => t.Id)
                .ToList();
        }

        public void SetTaskStatus(int userId, int taskId, bool isCompleted)
        {
            var status = _db.UserTaskStatuses
                .FirstOrDefault(s => s.UserId == userId && s.OnboardingTaskId == taskId);

            if (status == null)
            {
                // 새로 생성
                status = new UserTaskStatus
                {
                    UserId = userId,
                    OnboardingTaskId = taskId,
                    IsCompleted = isCompleted
                };
                _db.UserTaskStatuses.Add(status);
            }
            else
            {
                // 업데이트
                status.IsCompleted = isCompleted;
            }

            _db.SaveChanges();
        }

        public bool GetTaskStatus(int userId, int taskId)
        {
            var status = _db.UserTaskStatuses
                .AsNoTracking()
                .FirstOrDefault(s => s.UserId == userId && s.OnboardingTaskId == taskId);

            return status?.IsCompleted ?? false;
        }

        public (int completed, int total) GetOverallProgress(int userId)
        {
            // 해당 사용자에게 할당된 태스크만 카운트
            var total = _db.OnboardingTasks
                .AsNoTracking()
                .Count(t => t.UserId == userId);

            if (total == 0) return (0, 0);

            var completed = _db.UserTaskStatuses
                .AsNoTracking()
                .Count(s => s.UserId == userId && s.IsCompleted);

            return (completed, total);
        }

        public bool HasDistributedTasks(int userId)
        {
            return _db.OnboardingTasks
                .AsNoTracking()
                .Any(t => t.UserId == userId);
        }

        public OnboardingTask AssignManualToUser(int userId, int manualId, int day)
        {
            // 매뉴얼 조회
            var manual = _db.Manuals
                .AsNoTracking()
                .FirstOrDefault(m => m.Id == manualId);

            if (manual == null)
            {
                throw new ArgumentException($"매뉴얼 ID {manualId}를 찾을 수 없습니다.");
            }

            // 해당 Day의 마지막 OrderIndex 조회 (클라이언트 측 평가)
            var orderIndices = _db.OnboardingTasks
                .AsNoTracking()
                .Where(t => t.UserId == userId && t.Day == day)
                .Select(t => t.OrderIndex)
                .ToList();
            var lastOrderIndex = orderIndices.Count > 0 ? orderIndices.Max() : 0;

            // 새 태스크 생성
            var task = new OnboardingTask
            {
                UserId = userId,
                Day = day,
                Title = manual.Title,
                ManualId = manualId,
                OrderIndex = lastOrderIndex + 1,
                CreatedAt = DateTime.Now
            };

            _db.OnboardingTasks.Add(task);
            _db.SaveChanges();

            System.Diagnostics.Debug.WriteLine($"[매뉴얼 분배] UserId={userId}, ManualId={manualId}, Day={day}, TaskId={task.Id}");
            return task;
        }

        public bool DeleteTask(int taskId)
        {
            var task = _db.OnboardingTasks.Find(taskId);
            if (task == null) return false;

            // 관련 UserTaskStatus도 삭제
            var statuses = _db.UserTaskStatuses
                .Where(s => s.OnboardingTaskId == taskId)
                .ToList();

            _db.UserTaskStatuses.RemoveRange(statuses);
            _db.OnboardingTasks.Remove(task);
            _db.SaveChanges();

            System.Diagnostics.Debug.WriteLine($"[태스크 삭제] TaskId={taskId}");
            return true;
        }

        public int DeleteAllUserTasks(int userId)
        {
            // 사용자의 모든 태스크 조회
            var tasks = _db.OnboardingTasks
                .Where(t => t.UserId == userId)
                .ToList();

            if (tasks.Count == 0) return 0;

            // 관련 UserTaskStatus 삭제
            var taskIds = tasks.Select(t => t.Id).ToList();
            var statuses = _db.UserTaskStatuses
                .Where(s => taskIds.Contains(s.OnboardingTaskId))
                .ToList();

            _db.UserTaskStatuses.RemoveRange(statuses);
            _db.OnboardingTasks.RemoveRange(tasks);
            _db.SaveChanges();

            System.Diagnostics.Debug.WriteLine($"[전체 태스크 삭제] UserId={userId}, 삭제된 태스크 수={tasks.Count}");
            return tasks.Count;
        }

        public Dictionary<int, int> GetTaskCountByDay(int userId)
        {
            return _db.OnboardingTasks
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .GroupBy(t => t.Day)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
