using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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

        // ==================== 비밀번호 해시 ====================

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            return HashPassword(password) == passwordHash;
        }

        // ==================== 사용자 생성 (비밀번호 포함) ====================

        public User CreateUser(string name, DateTime joinDate, string password, bool isAdmin = false)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ValidationException("이름", "이름은 필수입니다.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ValidationException("비밀번호", "비밀번호는 필수입니다.");
            }

            try
            {
                // 첫 번째 사용자는 자동으로 관리자로 설정
                var userCount = _db.Users.Count();
                if (userCount == 0)
                {
                    isAdmin = true;
                    System.Diagnostics.Debug.WriteLine("[첫 사용자 자동 관리자] 첫 번째 사용자를 관리자로 설정");
                }

                var user = new User
                {
                    Name = name,
                    JoinDate = joinDate,
                    IsAdmin = isAdmin,
                    PasswordHash = HashPassword(password)
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

        // ==================== 로그인 검증 ====================

        public User? ValidateLogin(string name, string password)
        {
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Name == name);
                if (user == null) return null;
                if (!VerifyPassword(password, user.PasswordHash)) return null;
                return user;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("로그인 검증 중 오류가 발생했습니다.", ex);
            }
        }

        public bool UserExists(string name)
        {
            try
            {
                return _db.Users.Any(u => u.Name == name);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("사용자 존재 여부 확인 중 오류가 발생했습니다.", ex);
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

        // ==================== 비동기 메서드 ====================

        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                return await _db.Users
                    .AsNoTracking()
                    .OrderBy(u => u.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DatabaseException("사용자 목록 조회 중 오류가 발생했습니다.", ex);
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("사용자 조회 중 오류가 발생했습니다.", ex);
            }
        }

        public async Task<bool> SetAdminStatusAsync(int userId, bool isAdmin)
        {
            try
            {
                var user = await _db.Users.FindAsync(userId);
                if (user == null)
                {
                    return false;
                }

                user.IsAdmin = isAdmin;
                await _db.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"[관리자 권한 변경] UserId={userId}, IsAdmin={isAdmin}");
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("관리자 권한 변경 중 오류가 발생했습니다.", ex);
            }
        }

        public async Task<User?> ValidateLoginAsync(string name, string password)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Name == name);
                if (user == null) return null;
                if (!VerifyPassword(password, user.PasswordHash)) return null;
                return user;
            }
            catch (Exception ex)
            {
                throw new DatabaseException("로그인 검증 중 오류가 발생했습니다.", ex);
            }
        }

        public async Task<bool> UserExistsAsync(string name)
        {
            try
            {
                return await _db.Users.AnyAsync(u => u.Name == name);
            }
            catch (Exception ex)
            {
                throw new DatabaseException("사용자 존재 여부 확인 중 오류가 발생했습니다.", ex);
            }
        }

        public async Task<User> CreateUserAsync(string name, DateTime joinDate, string password, bool isAdmin = false)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ValidationException("이름", "이름은 필수입니다.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ValidationException("비밀번호", "비밀번호는 필수입니다.");
            }

            try
            {
                // 첫 번째 사용자는 자동으로 관리자로 설정
                var userCount = await _db.Users.CountAsync();
                if (userCount == 0)
                {
                    isAdmin = true;
                    System.Diagnostics.Debug.WriteLine("[첫 사용자 자동 관리자] 첫 번째 사용자를 관리자로 설정");
                }

                var user = new User
                {
                    Name = name,
                    JoinDate = joinDate,
                    IsAdmin = isAdmin,
                    PasswordHash = HashPassword(password)
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                return user;
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException("사용자 생성 중 오류가 발생했습니다.", ex);
            }
        }
    }
}
