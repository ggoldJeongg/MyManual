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
        public User CreateUser(string name, DateTime joinDate, bool isAdmin = false)
        {
            var db = AppDbContext.Instance;

            var user = new User
            {
                Name = name,
                JoinDate = joinDate,
                IsAdmin = isAdmin
            };

            db.Users.Add(user);
            db.SaveChanges();
            db.ChangeTracker.Clear();

            return user;
        }

        public User? GetUserById(int id)
        {
            var db = AppDbContext.Instance;
            return db.Users.AsNoTracking().FirstOrDefault(u => u.Id == id);
        }

        public User? GetUserByName(string name)
        {
            var db = AppDbContext.Instance;
            return db.Users.AsNoTracking().FirstOrDefault(u => u.Name == name);
        }

        /// <summary>
        /// JSON에서 로드한 사용자를 DB와 동기화
        /// DB에 없으면 생성, 있으면 업데이트 후 DB의 User 반환
        /// </summary>
        public User SyncUser(User jsonUser)
        {
            var db = AppDbContext.Instance;

            // 이름으로 기존 사용자 찾기
            var existingUser = db.Users.FirstOrDefault(u => u.Name == jsonUser.Name);

            if (existingUser == null)
            {
                // DB에 없으면 새로 생성
                var newUser = new User
                {
                    Name = jsonUser.Name,
                    JoinDate = jsonUser.JoinDate,
                    IsAdmin = jsonUser.IsAdmin
                };

                db.Users.Add(newUser);
                db.SaveChanges();
                db.ChangeTracker.Clear();

                System.Diagnostics.Debug.WriteLine($"[사용자 동기화] 새 사용자 생성: Id={newUser.Id}, Name={newUser.Name}");
                return newUser;
            }
            else
            {
                // DB에 있으면 정보 업데이트
                existingUser.JoinDate = jsonUser.JoinDate;
                existingUser.IsAdmin = jsonUser.IsAdmin;
                db.SaveChanges();
                db.ChangeTracker.Clear();

                System.Diagnostics.Debug.WriteLine($"[사용자 동기화] 기존 사용자 업데이트: Id={existingUser.Id}, Name={existingUser.Name}");
                return existingUser;
            }
        }

        public User UpdateUser(User user)
        {
            var db = AppDbContext.Instance;

            var existing = db.Users.Find(user.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"사용자 ID {user.Id}를 찾을 수 없습니다.");
            }

            existing.Name = user.Name;
            existing.JoinDate = user.JoinDate;
            existing.IsAdmin = user.IsAdmin;

            db.SaveChanges();
            db.ChangeTracker.Clear();

            return existing;
        }

        public bool IsAdmin(int userId)
        {
            var db = AppDbContext.Instance;
            var user = db.Users.AsNoTracking().FirstOrDefault(u => u.Id == userId);
            return user?.IsAdmin ?? false;
        }
    }
}
