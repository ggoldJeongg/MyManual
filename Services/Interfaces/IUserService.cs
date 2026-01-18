using System.Collections.Generic;
using MyManual.Models;

namespace MyManual.Services.Interfaces
{
    /// <summary>
    /// 사용자 관련 비즈니스 로직 인터페이스
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 사용자 등록
        /// </summary>
        User CreateUser(string name, DateTime joinDate, bool isAdmin = false);

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
        /// <param name="userId">대상 사용자 ID</param>
        /// <param name="isAdmin">관리자 여부</param>
        /// <returns>변경 성공 여부</returns>
        bool SetAdminStatus(int userId, bool isAdmin);
    }
}
