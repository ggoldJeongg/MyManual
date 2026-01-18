using System.Threading.Tasks;
using MyManual.Models;

namespace MyManual.Services.Interfaces
{
    /// <summary>
    /// 매뉴얼 관련 비즈니스 로직 인터페이스
    /// </summary>
    public interface IManualService
    {
        // ==================== 동기 메서드 ====================

        /// <summary>
        /// 모든 매뉴얼 조회
        /// </summary>
        List<Manual> GetAllManuals();

        /// <summary>
        /// ID로 매뉴얼 조회
        /// </summary>
        Manual? GetManualById(int id);

        /// <summary>
        /// 카테고리별 매뉴얼 조회
        /// </summary>
        List<Manual> GetManualsByCategory(string category);

        /// <summary>
        /// 매뉴얼 생성 (관리자만)
        /// </summary>
        Manual CreateManual(Manual manual, User user);

        /// <summary>
        /// 매뉴얼 수정 (관리자만)
        /// </summary>
        Manual UpdateManual(Manual manual, User user);

        /// <summary>
        /// 매뉴얼 삭제 (관리자만)
        /// </summary>
        bool DeleteManual(int id, User user);

        /// <summary>
        /// 사용자의 체크리스트 완료 상태 조회
        /// </summary>
        List<UserChecklistStatus> GetUserChecklistStatuses(int userId, int manualId);

        /// <summary>
        /// 체크리스트 항목 체크/해제
        /// </summary>
        void SetChecklistStatus(int userId, int checklistItemId, bool isChecked);

        /// <summary>
        /// 매뉴얼의 체크리스트 진행률 조회
        /// </summary>
        (int completed, int total) GetChecklistProgress(int userId, int manualId);

        // ==================== 비동기 메서드 ====================

        /// <summary>
        /// 모든 매뉴얼 조회 (비동기)
        /// </summary>
        Task<List<Manual>> GetAllManualsAsync();

        /// <summary>
        /// ID로 매뉴얼 조회 (비동기)
        /// </summary>
        Task<Manual?> GetManualByIdAsync(int id);

        /// <summary>
        /// 매뉴얼 생성 (비동기)
        /// </summary>
        Task<Manual> CreateManualAsync(Manual manual, User user);

        /// <summary>
        /// 매뉴얼 수정 (비동기)
        /// </summary>
        Task<Manual> UpdateManualAsync(Manual manual, User user);

        /// <summary>
        /// 매뉴얼 삭제 (비동기)
        /// </summary>
        Task<bool> DeleteManualAsync(int id, User user);

        /// <summary>
        /// 체크리스트 항목 체크/해제 (비동기)
        /// </summary>
        Task SetChecklistStatusAsync(int userId, int checklistItemId, bool isChecked);
    }
}
