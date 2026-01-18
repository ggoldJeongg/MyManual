using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MyManual.Data;
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

        /// <summary>
        /// 비밀번호를 SHA256으로 해시
        /// </summary>
        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 비밀번호 검증
        /// </summary>
        public bool VerifyPassword(string password, string passwordHash)
        {
            return HashPassword(password) == passwordHash;
        }

        public User CreateUser(string name, DateTime joinDate, string password, bool isAdmin = false)
        {
            // 첫 사용자는 자동으로 관리자로 설정
            bool shouldBeAdmin = isAdmin;
            if (!_db.Users.Any())
            {
                shouldBeAdmin = true;
                System.Diagnostics.Debug.WriteLine($"[첫 사용자] '{name}'을(를) 관리자로 자동 설정");
            }

            var user = new User
            {
                Name = name,
                JoinDate = joinDate,
                PasswordHash = HashPassword(password),
                IsAdmin = shouldBeAdmin
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            System.Diagnostics.Debug.WriteLine($"[사용자 생성] Id={user.Id}, Name={user.Name}, IsAdmin={user.IsAdmin}");
            return user;
        }

        /// <summary>
        /// 로그인 검증 (이름 + 비밀번호)
        /// </summary>
        public User? ValidateLogin(string name, string password)
        {
            var user = _db.Users.FirstOrDefault(u => u.Name == name);
            if (user == null)
            {
                return null;
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }

            return user;
        }

        /// <summary>
        /// 사용자 존재 여부 확인
        /// </summary>
        public bool UserExists(string name)
        {
            return _db.Users.Any(u => u.Name == name);
        }

        public User? GetUserById(int id)
        {
            return _db.Users.AsNoTracking().FirstOrDefault(u => u.Id == id);
        }

        public User? GetUserByName(string name)
        {
            return _db.Users.AsNoTracking().FirstOrDefault(u => u.Name == name);
        }

        /// <summary>
        /// JSON에서 로드한 사용자를 DB와 동기화
        /// DB에 없으면 생성, 있으면 업데이트 후 DB의 User 반환
        /// </summary>
        public User SyncUser(User jsonUser)
        {
            // 이름으로 기존 사용자 찾기
            var existingUser = _db.Users.FirstOrDefault(u => u.Name == jsonUser.Name);

            if (existingUser == null)
            {
                // 첫 사용자는 자동으로 관리자로 설정
                bool shouldBeAdmin = jsonUser.IsAdmin;
                if (!_db.Users.Any())
                {
                    shouldBeAdmin = true;
                    System.Diagnostics.Debug.WriteLine($"[첫 사용자] '{jsonUser.Name}'을(를) 관리자로 자동 설정");
                }

                // DB에 없으면 새로 생성
                var newUser = new User
                {
                    Name = jsonUser.Name,
                    JoinDate = jsonUser.JoinDate,
                    IsAdmin = shouldBeAdmin
                };

                _db.Users.Add(newUser);
                _db.SaveChanges();

                System.Diagnostics.Debug.WriteLine($"[사용자 동기화] 새 사용자 생성: Id={newUser.Id}, Name={newUser.Name}, IsAdmin={newUser.IsAdmin}");
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

        public User UpdateUser(User user)
        {
            var existing = _db.Users.Find(user.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"사용자 ID {user.Id}를 찾을 수 없습니다.");
            }

            existing.Name = user.Name;
            existing.JoinDate = user.JoinDate;
            existing.IsAdmin = user.IsAdmin;

            _db.SaveChanges();

            return existing;
        }

        public bool IsAdmin(int userId)
        {
            var user = _db.Users.AsNoTracking().FirstOrDefault(u => u.Id == userId);
            return user?.IsAdmin ?? false;
        }

        public List<User> GetAllUsers()
        {
            return _db.Users
                .AsNoTracking()
                .OrderBy(u => u.Name)
                .ToList();
        }

        public bool SetAdminStatus(int userId, bool isAdmin)
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
    }
}
