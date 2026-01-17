using MyManual.Models;
using MyManual.ViewModels;

namespace MyManual.Services.Interfaces
{
    /// <summary>
    /// 온보딩 태스크 관련 비즈니스 로직 인터페이스
    /// </summary>
    public interface IOnboardingService
    {
        /// <summary>
        /// 특정 Day의 온보딩 태스크 조회 (사용자 완료 상태 포함)
        /// </summary>
        List<OnboardingTaskViewModel> GetTasksForDay(int day, int userId);

        /// <summary>
        /// 모든 온보딩 태스크 조회
        /// </summary>
        List<OnboardingTask> GetAllTasks();

        /// <summary>
        /// 태스크 완료 상태 변경
        /// </summary>
        void SetTaskStatus(int userId, int taskId, bool isCompleted);

        /// <summary>
        /// 사용자의 태스크 완료 상태 조회
        /// </summary>
        bool GetTaskStatus(int userId, int taskId);

        /// <summary>
        /// 사용자의 전체 진행률 조회
        /// </summary>
        (int completed, int total) GetOverallProgress(int userId);
    }
}
