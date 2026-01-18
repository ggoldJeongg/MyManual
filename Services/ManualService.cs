using Microsoft.EntityFrameworkCore;
using MyManual.Data;
using MyManual.Models;
using MyManual.Services.Interfaces;

namespace MyManual.Services
{
    /// <summary>
    /// 매뉴얼 관련 비즈니스 로직 + DB 접근
    /// </summary>
    public class ManualService : IManualService
    {
        private readonly AppDbContext _db;

        public ManualService(AppDbContext db)
        {
            _db = db;
        }

        // ==================== 매뉴얼 CRUD ====================

        public List<Manual> GetAllManuals()
        {
            return _db.Manuals
                .AsNoTracking()
                .Include(m => m.Checklist)
                .Include(m => m.History)
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Title)
                .ToList();
        }

        public Manual? GetManualById(int id)
        {
            return _db.Manuals
                .AsNoTracking()
                .Include(m => m.Checklist.OrderBy(c => c.OrderIndex))
                .Include(m => m.History.OrderByDescending(h => h.Date))
                .FirstOrDefault(m => m.Id == id);
        }

        public List<Manual> GetManualsByCategory(string category)
        {
            return _db.Manuals
                .AsNoTracking()
                .Include(m => m.Checklist)
                .Include(m => m.History)
                .Where(m => m.Category == category)
                .OrderBy(m => m.Title)
                .ToList();
        }

        public Manual CreateManual(Manual manual, User user)
        {
            System.Diagnostics.Debug.WriteLine($"[CreateManual] 시작 - UserId={user.Id}, IsAdmin={user.IsAdmin}");

            // 권한 체크
            if (!user.IsAdmin)
            {
                throw new UnauthorizedAccessException("관리자만 매뉴얼을 생성할 수 있습니다.");
            }

            manual.CreatedAt = DateTime.Now;
            manual.UpdatedAt = DateTime.Now;

            // 체크리스트 OrderIndex 설정
            for (int i = 0; i < manual.Checklist.Count; i++)
            {
                manual.Checklist.ElementAt(i).OrderIndex = i;
            }

            System.Diagnostics.Debug.WriteLine($"[CreateManual] DB Add 시작");
            _db.Manuals.Add(manual);
            System.Diagnostics.Debug.WriteLine($"[CreateManual] DB Add 완료, SaveChanges 시작");
            _db.SaveChanges();
            System.Diagnostics.Debug.WriteLine($"[CreateManual] SaveChanges 완료 - ManualId={manual.Id}");

            return manual;
        }

        public Manual UpdateManual(Manual manual, User user)
        {
            // 권한 체크
            if (!user.IsAdmin)
            {
                throw new UnauthorizedAccessException("관리자만 매뉴얼을 수정할 수 있습니다.");
            }

            var existing = _db.Manuals
                .Include(m => m.Checklist)
                .Include(m => m.History)
                .FirstOrDefault(m => m.Id == manual.Id);

            if (existing == null)
            {
                throw new KeyNotFoundException($"매뉴얼 ID {manual.Id}를 찾을 수 없습니다.");
            }

            // 기본 정보 업데이트
            existing.Title = manual.Title;
            existing.Category = manual.Category;
            existing.Purpose = manual.Purpose;
            existing.Process = manual.Process;
            existing.UpdatedAt = DateTime.Now;

            // 히스토리 추가
            existing.History.Add(new HistoryItem
            {
                ManualId = existing.Id,
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
                Description = "매뉴얼 수정됨"
            });

            _db.SaveChanges();

            return existing;
        }

        public bool DeleteManual(int id, User user)
        {
            // 권한 체크
            if (!user.IsAdmin)
            {
                throw new UnauthorizedAccessException("관리자만 매뉴얼을 삭제할 수 있습니다.");
            }

            var manual = _db.Manuals.Find(id);
            if (manual == null)
            {
                return false;
            }

            _db.Manuals.Remove(manual);
            _db.SaveChanges();

            return true;
        }

        // ==================== 체크리스트 상태 ====================

        public List<UserChecklistStatus> GetUserChecklistStatuses(int userId, int manualId)
        {
            var checklistItemIds = _db.ChecklistItems
                .AsNoTracking()
                .Where(c => c.ManualId == manualId)
                .Select(c => c.Id)
                .ToList();

            return _db.UserChecklistStatuses
                .AsNoTracking()
                .Where(s => s.UserId == userId && checklistItemIds.Contains(s.ChecklistItemId))
                .ToList();
        }

        public void SetChecklistStatus(int userId, int checklistItemId, bool isChecked)
        {
            var status = _db.UserChecklistStatuses
                .FirstOrDefault(s => s.UserId == userId && s.ChecklistItemId == checklistItemId);

            if (status == null)
            {
                // 새로 생성
                status = new UserChecklistStatus
                {
                    UserId = userId,
                    ChecklistItemId = checklistItemId,
                    IsChecked = isChecked
                };
                _db.UserChecklistStatuses.Add(status);
            }
            else
            {
                // 업데이트
                status.IsChecked = isChecked;
            }

            _db.SaveChanges();
        }

        public (int completed, int total) GetChecklistProgress(int userId, int manualId)
        {
            var checklistItems = _db.ChecklistItems
                .AsNoTracking()
                .Where(c => c.ManualId == manualId)
                .ToList();

            var total = checklistItems.Count;

            if (total == 0) return (0, 0);

            var checklistItemIds = checklistItems.Select(c => c.Id).ToList();

            var completed = _db.UserChecklistStatuses
                .Count(s => s.UserId == userId
                         && checklistItemIds.Contains(s.ChecklistItemId)
                         && s.IsChecked);

            return (completed, total);
        }
    }
}
