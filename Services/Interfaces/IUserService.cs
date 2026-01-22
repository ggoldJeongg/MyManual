using System.Collections.Generic;
using System.Threading.Tasks;
using MyManual.Models;

namespace MyManual.Services.Interfaces
{
    /// <summary>
    /// 사용자 관련 비즈니스 로직 인터페이스
    /// </summary>
    public interface IUserService
    {
        // ==================== 동기 메서드 ====================

        /// <summary>
        /// 사용자 등록 (비밀번호 포함)
        /// </summary>
        User CreateUser(string name, DateTime joinDate, string password, bool isAdmin = false);

        /// <summary>
        /// ID로 사용자 조회
        /// </summary>
        User? GetUserById(int id);

        /// <summary>
        /// 이름으로 사용자 조회
        /// </summary>
        User? GetUserByName(string name);

        /// <summary>
        /// 사용자 정보 수정
        /// </summary>
        User UpdateUser(User user);

        /// <summary>
        /// 관리자 여부 확인
        /// </summary>
        bool IsAdmin(int userId);

        /// <summary>
        /// 전체 사용자 목록 조회
        /// </summary>
        List<User> GetAllUsers();

        /// <summary>
        /// 관리자 권한 설정
        /// </summary>
        bool SetAdminStatus(int userId, bool isAdmin);

        // ==================== 비밀번호 관련 메서드 ====================

        /// <summary>
        /// 로그인 검증 (이름 + 비밀번호)
        /// </summary>
        User? ValidateLogin(string name, string password);

        /// <summary>
        /// 사용자 존재 여부 확인
        /// </summary>
        bool UserExists(string name);

        /// <summary>
        /// 비밀번호 검증
        /// </summary>
        bool VerifyPassword(string password, string passwordHash);

        // ==================== 비동기 메서드 ====================

        /// <summary>
        /// 전체 사용자 목록 조회 (비동기)
        /// </summary>
        Task<List<User>> GetAllUsersAsync();

        /// <summary>
        /// ID로 사용자 조회 (비동기)
        /// </summary>
        Task<User?> GetUserByIdAsync(int id);

        /// <summary>
        /// 관리자 권한 설정 (비동기)
        /// </summary>
        Task<bool> SetAdminStatusAsync(int userId, bool isAdmin);

        /// <summary>
        /// 로그인 검증 (비동기)
        /// </summary>
        Task<User?> ValidateLoginAsync(string name, string password);

        /// <summary>
        /// 사용자 존재 여부 확인 (비동기)
        /// </summary>
        Task<bool> UserExistsAsync(string name);

        /// <summary>
        /// 사용자 등록 (비동기)
        /// </summary>
        Task<User> CreateUserAsync(string name, DateTime joinDate, string password, bool isAdmin = false);
    }
}
