using Microsoft.EntityFrameworkCore;
using MyManual.Data;
using MyManual.Exceptions;
using MyManual.Models;
using MyManual.Services.Interfaces;

namespace MyManual.Services
{
    /// <summary>
    /// 사용자 관련 비즈니스 로직 + DB 접근
    /// </summary>
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;

        public UserService(AppDbContext db)
        {
            _db = db;
        }

        public User CreateUser(string name, DateTime joinDate, bool isAdmin = false)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ValidationException("이름", "이름은 필수입니다.");
            }

            try
            {
                var user = new User
                {
                    Name = name,
                    JoinDate = joinDate,
                    IsAdmin = isAdmin
                };

                _db.Users.Add(user);
                _db.SaveChanges();

                return user;
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("사용자 생성 중 오류가 발생했습니다.", ex);
            }
        }

        public User? GetUserById(int id)
        {
            try
            {
                return _db.Users.AsNoTracking().FirstOrDefault(u => u.Id == id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("사용자 조회 중 오류가 발생했습니다.", ex);
            }
        }

        public User? GetUserByName(string name)
        {
            try
            {
                return _db.Users.AsNoTracking().FirstOrDefault(u => u.Name == name);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("사용자 조회 중 오류가 발생했습니다.", ex);
            }
        }

        /// <summary>
        /// JSON에서 로드한 사용자를 DB와 동기화
        /// DB에 없으면 생성, 있으면 업데이트 후 DB의 User 반환
        /// </summary>
        public User SyncUser(User jsonUser)
        {
            if (jsonUser == null)
            {
                throw new ValidationException("사용자", "사용자 정보가 없습니다.");
            }

            try
            {
                // 이름으로 기존 사용자 찾기
                var existingUser = _db.Users.FirstOrDefault(u => u.Name == jsonUser.Name);

                if (existingUser == null)
                {
                    // DB에 없으면 새로 생성
                    var newUser = new User
                    {
                        Name = jsonUser.Name,
                        JoinDate = jsonUser.JoinDate,
                        IsAdmin = jsonUser.IsAdmin
                    };

                    _db.Users.Add(newUser);
                    _db.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"[사용자 동기화] 새 사용자 생성: Id={newUser.Id}, Name={newUser.Name}");
                    return newUser;
                }
                else
                {
                    // DB에 있으면 정보 업데이트
                    existingUser.JoinDate = jsonUser.JoinDate;
                    existingUser.IsAdmin = jsonUser.IsAdmin;
                    _db.SaveChanges();

                    System.Diagnostics.Debug.WriteLine($"[사용자 동기화] 기존 사용자 업데이트: Id={existingUser.Id}, Name={existingUser.Name}");
                    return existingUser;
                }
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("사용자 동기화 중 오류가 발생했습니다.", ex);
            }
        }

        public User UpdateUser(User user)
        {
            if (user == null)
            {
                throw new ValidationException("사용자", "사용자 정보가 없습니다.");
            }

            try
            {
                var existing = _db.Users.Find(user.Id);
                if (existing == null)
                {
                    throw new EntityNotFoundException("사용자", user.Id);
                }

                existing.Name = user.Name;
                existing.JoinDate = user.JoinDate;
                existing.IsAdmin = user.IsAdmin;

                _db.SaveChanges();

                return existing;
            }
            catch (EntityNotFoundException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("사용자 업데이트 중 오류가 발생했습니다.", ex);
            }
        }

        public bool IsAdmin(int userId)
        {
            try
            {
                var user = _db.Users.AsNoTracking().FirstOrDefault(u => u.Id == userId);
                return user?.IsAdmin ?? false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[IsAdmin 오류] {ex.Message}");
                return false;
            }
        }

        public List<User> GetAllUsers()
        {
            try
            {
                return _db.Users
                    .AsNoTracking()
                    .OrderBy(u => u.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("사용자 목록 조회 중 오류가 발생했습니다.", ex);
            }
        }

        public bool SetAdminStatus(int userId, bool isAdmin)
        {
            try
            {
                var user = _db.Users.Find(userId);
                if (user == null)
                {
                    return false;
                }

                user.IsAdmin = isAdmin;
                _db.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"[관리자 권한 변경] UserId={userId}, IsAdmin={isAdmin}");
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("관리자 권한 변경 중 오류가 발생했습니다.", ex);
            }
        }
    }
}
