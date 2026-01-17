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
        public List<OnboardingTaskViewModel> GetTasksForDay(int day, int userId)
        {
            var db = AppDbContext.Instance;

            var tasks = db.OnboardingTasks
                .AsNoTracking()
                .Where(t => t.Day == day)
                .OrderBy(t => t.Id)
                .ToList();

            // 사용자 완료 상태 조회
            var taskIds = tasks.Select(t => t.Id).ToList();
            var statuses = db.UserTaskStatuses
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

        public List<OnboardingTask> GetAllTasks()
        {
            var db = AppDbContext.Instance;
            return db.OnboardingTasks
                .AsNoTracking()
                .OrderBy(t => t.Day)
                .ThenBy(t => t.Id)
                .ToList();
        }

        public void SetTaskStatus(int userId, int taskId, bool isCompleted)
        {
            var db = AppDbContext.Instance;

            var status = db.UserTaskStatuses
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
                db.UserTaskStatuses.Add(status);
            }
            else
            {
                // 업데이트
                status.IsCompleted = isCompleted;
            }

            db.SaveChanges();
            db.ChangeTracker.Clear();
        }

        public bool GetTaskStatus(int userId, int taskId)
        {
            var db = AppDbContext.Instance;

            var status = db.UserTaskStatuses
                .AsNoTracking()
                .FirstOrDefault(s => s.UserId == userId && s.OnboardingTaskId == taskId);

            return status?.IsCompleted ?? false;
        }

        public (int completed, int total) GetOverallProgress(int userId)
        {
            var db = AppDbContext.Instance;

            var total = db.OnboardingTasks.AsNoTracking().Count();

            if (total == 0) return (0, 0);

            var completed = db.UserTaskStatuses
                .AsNoTracking()
                .Count(s => s.UserId == userId && s.IsCompleted);

            return (completed, total);
        }
    }
}
