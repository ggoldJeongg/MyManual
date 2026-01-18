using MyManual.Models;

namespace MyManual.Services.Interfaces
{
    /// <summary>
    /// 매뉴얼 관련 비즈니스 로직 인터페이스
    /// </summary>
    public interface IManualService
    {
        // ==================== 매뉴얼 CRUD ====================

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

        // ==================== 체크리스트 상태 ====================

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
    }
}
