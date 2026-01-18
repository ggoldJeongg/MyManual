using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyManual.Data;
using MyManual.Exceptions;
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
            try
            {
                return _db.Manuals
                    .AsNoTracking()
                    .Include(m => m.Checklist)
                    .Include(m => m.History)
                    .OrderBy(m => m.Category)
                    .ThenBy(m => m.Title)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("매뉴얼 목록 조회 중 오류가 발생했습니다.", ex);
            }
        }

        public Manual? GetManualById(int id)
        {
            try
            {
                return _db.Manuals
                    .AsNoTracking()
                    .Include(m => m.Checklist.OrderBy(c => c.OrderIndex))
                    .Include(m => m.History.OrderByDescending(h => h.Date))
                    .FirstOrDefault(m => m.Id == id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("매뉴얼 조회 중 오류가 발생했습니다.", ex);
            }
        }

        public List<Manual> GetManualsByCategory(string category)
        {
            try
            {
                return _db.Manuals
                    .AsNoTracking()
                    .Include(m => m.Checklist)
                    .Include(m => m.History)
                    .Where(m => m.Category == category)
                    .OrderBy(m => m.Title)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("카테고리별 매뉴얼 조회 중 오류가 발생했습니다.", ex);
            }
        }

        public Manual CreateManual(Manual manual, User user)
        {
            if (manual == null)
            {
                throw new ValidationException("매뉴얼", "매뉴얼 정보가 없습니다.");
            }

            if (string.IsNullOrWhiteSpace(manual.Title))
            {
                throw new ValidationException("제목", "제목은 필수입니다.");
            }

            if (user == null)
            {
                throw new ValidationException("사용자", "사용자 정보가 없습니다.");
            }

            // 권한 체크
            if (!user.IsAdmin)
            {
                throw new UnauthorizedException("관리자만 매뉴얼을 생성할 수 있습니다.");
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"[CreateManual] 시작 - UserId={user.Id}, IsAdmin={user.IsAdmin}");

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
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("매뉴얼 생성 중 오류가 발생했습니다.", ex);
            }
        }

        public Manual UpdateManual(Manual manual, User user)
        {
            if (manual == null)
            {
                throw new ValidationException("매뉴얼", "매뉴얼 정보가 없습니다.");
            }

            if (user == null)
            {
                throw new ValidationException("사용자", "사용자 정보가 없습니다.");
            }

            // 권한 체크
            if (!user.IsAdmin)
            {
                throw new UnauthorizedException("관리자만 매뉴얼을 수정할 수 있습니다.");
            }

            try
            {
                var existing = _db.Manuals
                    .Include(m => m.Checklist)
                    .Include(m => m.History)
                    .FirstOrDefault(m => m.Id == manual.Id);

                if (existing == null)
                {
                    throw new EntityNotFoundException("매뉴얼", manual.Id);
                }

                // 기본 정보 업데이트
                existing.Title = manual.Title;
                existing.Category = manual.Category;
                existing.Purpose = manual.Purpose;
                existing.Process = manual.Process;
                existing.UpdatedAt = DateTime.Now;

                // 기존 체크리스트 삭제 후 새로 추가
                _db.ChecklistItems.RemoveRange(existing.Checklist);
                existing.Checklist.Clear();

                foreach (var item in manual.Checklist)
                {
                    existing.Checklist.Add(new ChecklistItem
                    {
                        ManualId = existing.Id,
                        Content = item.Content
                    });
                }

                // 히스토리 추가 (전달받은 히스토리 사용)
                foreach (var historyItem in manual.History.Where(h => h.Id == 0))
                {
                    existing.History.Add(new HistoryItem
                    {
                        ManualId = existing.Id,
                        Date = historyItem.Date,
                        Description = historyItem.Description
                    });
                }

                _db.SaveChanges();

                return existing;
            }
            catch (EntityNotFoundException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("매뉴얼 수정 중 오류가 발생했습니다.", ex);
            }
        }

        public bool DeleteManual(int id, User user)
        {
            if (user == null)
            {
                throw new ValidationException("사용자", "사용자 정보가 없습니다.");
            }

            // 권한 체크
            if (!user.IsAdmin)
            {
                throw new UnauthorizedException("관리자만 매뉴얼을 삭제할 수 있습니다.");
            }

            try
            {
                var manual = _db.Manuals.Find(id);
                if (manual == null)
                {
                    return false;
                }

                _db.Manuals.Remove(manual);
                _db.SaveChanges();

                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("매뉴얼 삭제 중 오류가 발생했습니다.", ex);
            }
        }

        // ==================== 체크리스트 상태 ====================

        public List<UserChecklistStatus> GetUserChecklistStatuses(int userId, int manualId)
        {
            try
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
            catch (Exception ex)
            {
                throw new DatabaseException("체크리스트 상태 조회 중 오류가 발생했습니다.", ex);
            }
        }

        public void SetChecklistStatus(int userId, int checklistItemId, bool isChecked)
        {
            try
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
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("체크리스트 상태 저장 중 오류가 발생했습니다.", ex);
            }
        }

        public (int completed, int total) GetChecklistProgress(int userId, int manualId)
        {
            try
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
            catch (Exception ex)
            {
                throw new DatabaseException("체크리스트 진행률 조회 중 오류가 발생했습니다.", ex);
            }
        }

        // ==================== 비동기 메서드 ====================

        public async Task<List<Manual>> GetAllManualsAsync()
        {
            try
            {
                return await _db.Manuals
                    .AsNoTracking()
                    .Include(m => m.Checklist)
                    .Include(m => m.History)
                    .OrderBy(m => m.Category)
                    .ThenBy(m => m.Title)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("매뉴얼 목록 조회 중 오류가 발생했습니다.", ex);
            }
        }

        public async Task<Manual?> GetManualByIdAsync(int id)
        {
            try
            {
                return await _db.Manuals
                    .AsNoTracking()
                    .Include(m => m.Checklist.OrderBy(c => c.OrderIndex))
                    .Include(m => m.History.OrderByDescending(h => h.Date))
                    .FirstOrDefaultAsync(m => m.Id == id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("매뉴얼 조회 중 오류가 발생했습니다.", ex);
            }
        }

        public async Task<Manual> CreateManualAsync(Manual manual, User user)
        {
            if (manual == null)
            {
                throw new ValidationException("매뉴얼", "매뉴얼 정보가 없습니다.");
            }

            if (string.IsNullOrWhiteSpace(manual.Title))
            {
                throw new ValidationException("제목", "제목은 필수입니다.");
            }

            if (user == null)
            {
                throw new ValidationException("사용자", "사용자 정보가 없습니다.");
            }

            if (!user.IsAdmin)
            {
                throw new UnauthorizedException("관리자만 매뉴얼을 생성할 수 있습니다.");
            }

            try
            {
                manual.CreatedAt = DateTime.Now;
                manual.UpdatedAt = DateTime.Now;

                for (int i = 0; i < manual.Checklist.Count; i++)
                {
                    manual.Checklist.ElementAt(i).OrderIndex = i;
                }

                _db.Manuals.Add(manual);
                await _db.SaveChangesAsync();

                return manual;
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("매뉴얼 생성 중 오류가 발생했습니다.", ex);
            }
        }

        public async Task<Manual> UpdateManualAsync(Manual manual, User user)
        {
            if (manual == null)
            {
                throw new ValidationException("매뉴얼", "매뉴얼 정보가 없습니다.");
            }

            if (user == null)
            {
                throw new ValidationException("사용자", "사용자 정보가 없습니다.");
            }

            if (!user.IsAdmin)
            {
                throw new UnauthorizedException("관리자만 매뉴얼을 수정할 수 있습니다.");
            }

            try
            {
                var existing = await _db.Manuals
                    .Include(m => m.Checklist)
                    .Include(m => m.History)
                    .FirstOrDefaultAsync(m => m.Id == manual.Id);

                if (existing == null)
                {
                    throw new EntityNotFoundException("매뉴얼", manual.Id);
                }

                existing.Title = manual.Title;
                existing.Category = manual.Category;
                existing.Purpose = manual.Purpose;
                existing.Process = manual.Process;
                existing.UpdatedAt = DateTime.Now;

                _db.ChecklistItems.RemoveRange(existing.Checklist);
                existing.Checklist.Clear();

                foreach (var item in manual.Checklist)
                {
                    existing.Checklist.Add(new ChecklistItem
                    {
                        ManualId = existing.Id,
                        Content = item.Content
                    });
                }

                foreach (var historyItem in manual.History.Where(h => h.Id == 0))
                {
                    existing.History.Add(new HistoryItem
                    {
                        ManualId = existing.Id,
                        Date = historyItem.Date,
                        Description = historyItem.Description
                    });
                }

                await _db.SaveChangesAsync();

                return existing;
            }
            catch (EntityNotFoundException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("매뉴얼 수정 중 오류가 발생했습니다.", ex);
            }
        }

        public async Task<bool> DeleteManualAsync(int id, User user)
        {
            if (user == null)
            {
                throw new ValidationException("사용자", "사용자 정보가 없습니다.");
            }

            if (!user.IsAdmin)
            {
                throw new UnauthorizedException("관리자만 매뉴얼을 삭제할 수 있습니다.");
            }

            try
            {
                var manual = await _db.Manuals.FindAsync(id);
                if (manual == null)
                {
                    return false;
                }

                _db.Manuals.Remove(manual);
                await _db.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("매뉴얼 삭제 중 오류가 발생했습니다.", ex);
            }
        }

        public async Task SetChecklistStatusAsync(int userId, int checklistItemId, bool isChecked)
        {
            try
            {
                var status = await _db.UserChecklistStatuses
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.ChecklistItemId == checklistItemId);

                if (status == null)
                {
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
                    status.IsChecked = isChecked;
                }

                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("체크리스트 상태 저장 중 오류가 발생했습니다.", ex);
            }
        }
    }
}
