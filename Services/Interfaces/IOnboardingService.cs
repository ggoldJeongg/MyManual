using System.Threading.Tasks;
using MyManual.Models;
using MyManual.ViewModels;

namespace MyManual.Services.Interfaces
{
    /// <summary>
    /// 온보딩 태스크 관련 비즈니스 로직 인터페이스
    /// </summary>
    public interface IOnboardingService
    {
        // ==================== 동기 메서드 ====================

        /// <summary>
        /// 특정 사용자의 특정 Day 온보딩 태스크 조회 (완료 상태 포함)
        /// </summary>
        List<OnboardingTaskViewModel> GetTasksForDay(int day, int userId);

        /// <summary>
        /// 특정 사용자의 모든 온보딩 태스크 조회
        /// </summary>
        List<OnboardingTask> GetUserTasks(int userId);

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

        /// <summary>
        /// 사용자에게 할일이 분배되었는지 확인
        /// </summary>
        bool HasDistributedTasks(int userId);

        /// <summary>
        /// 사용자에게 특정 매뉴얼을 특정 Day에 분배
        /// </summary>
        OnboardingTask AssignManualToUser(int userId, int manualId, int day);

        /// <summary>
        /// 사용자의 특정 태스크 삭제
        /// </summary>
        bool DeleteTask(int taskId);

        /// <summary>
        /// 사용자의 모든 태스크 삭제
        /// </summary>
        int DeleteAllUserTasks(int userId);

        /// <summary>
        /// 사용자의 Day별 태스크 개수 조회
        /// </summary>
        Dictionary<int, int> GetTaskCountByDay(int userId);

        // ==================== 비동기 메서드 ====================

        /// <summary>
        /// 특정 사용자의 특정 Day 온보딩 태스크 조회 (비동기)
        /// </summary>
        Task<List<OnboardingTaskViewModel>> GetTasksForDayAsync(int day, int userId);

        /// <summary>
        /// 특정 사용자의 모든 온보딩 태스크 조회 (비동기)
        /// </summary>
        Task<List<OnboardingTask>> GetUserTasksAsync(int userId);

        /// <summary>
        /// 태스크 완료 상태 변경 (비동기)
        /// </summary>
        Task SetTaskStatusAsync(int userId, int taskId, bool isCompleted);

        /// <summary>
        /// 사용자의 전체 진행률 조회 (비동기)
        /// </summary>
        Task<(int completed, int total)> GetOverallProgressAsync(int userId);

        /// <summary>
        /// 사용자에게 특정 매뉴얼을 특정 Day에 분배 (비동기)
        /// </summary>
        Task<OnboardingTask> AssignManualToUserAsync(int userId, int manualId, int day);

        /// <summary>
        /// 사용자의 특정 태스크 삭제 (비동기)
        /// </summary>
        Task<bool> DeleteTaskAsync(int taskId);

        /// <summary>
        /// 사용자의 모든 태스크 삭제 (비동기)
        /// </summary>
        Task<int> DeleteAllUserTasksAsync(int userId);
    }
}
