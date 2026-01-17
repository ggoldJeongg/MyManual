using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyManual.Data;
using MyManual.Models;
using MyManual.Services;
using MyManual.Services.Interfaces;

namespace MyManual
{
    public partial class App : Application
    {
        // ==================== DI 컨테이너 ====================

        public static IServiceProvider Services { get; private set; } = null!;

        // 앱 전역에서 사용할 현재 사용자
        public static User? CurrentUser { get; private set; }

        // 현재 로그인한 사용자 ID 저장 경로 (DB의 User Id만 저장)
        private static readonly string UserIdPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MyManual",
            "current_user_id.txt"
        );

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. DI 컨테이너 설정
            ConfigureServices();

            // 2. DB 초기화 (테이블 생성 + JSON 마이그레이션)
            var dbInitializer = Services.GetRequiredService<DatabaseInitializer>();
            dbInitializer.Initialize();

            // 3. DB에서 현재 사용자 로드 시도
            var userService = Services.GetRequiredService<IUserService>();
            var savedUserId = LoadCurrentUserId();

            if (savedUserId > 0)
            {
                CurrentUser = userService.GetUserById(savedUserId);
            }

            // 4. MainWindow 생성 및 표시
            var mainWindow = Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // DbContext 등록 (Transient: 매 요청마다 새 인스턴스)
            services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = $"Data Source={AppDbContext.DbPath};Mode=ReadWriteCreate;Cache=Shared";
                options.UseSqlite(connectionString);
            }, ServiceLifetime.Transient);

            // Services 등록
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IManualService, ManualService>();
            services.AddTransient<IOnboardingService, OnboardingService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddTransient<DatabaseInitializer>();

            // MainWindow 등록
            services.AddTransient<MainWindow>();

            Services = services.BuildServiceProvider();
        }

        // 사용자 정보 설정 (DB에 저장하고, 현재 사용자 ID만 파일에 저장)
        public static void SetCurrentUser(User user)
        {
            var userService = Services.GetRequiredService<IUserService>();

            // DB에 사용자가 있는지 확인
            var existingUser = userService.GetUserByName(user.Name);

            if (existingUser != null)
            {
                // 기존 사용자 - 정보 업데이트
                existingUser.JoinDate = user.JoinDate;
                CurrentUser = userService.UpdateUser(existingUser);
                System.Diagnostics.Debug.WriteLine($"[SetCurrentUser] 기존 사용자 업데이트: Id={CurrentUser.Id}, Name={CurrentUser.Name}");
            }
            else
            {
                // 새 사용자 - DB에 생성
                CurrentUser = userService.CreateUser(user.Name, user.JoinDate, user.IsAdmin);
                System.Diagnostics.Debug.WriteLine($"[SetCurrentUser] 새 사용자 생성: Id={CurrentUser.Id}, Name={CurrentUser.Name}");
            }

            // 현재 사용자 ID만 파일에 저장
            System.Diagnostics.Debug.WriteLine($"[SetCurrentUser] 저장할 UserId: {CurrentUser.Id}");
            SaveCurrentUserId(CurrentUser.Id);
        }

        // 현재 사용자 ID 파일에서 로드
        private static int LoadCurrentUserId()
        {
            try
            {
                if (File.Exists(UserIdPath))
                {
                    var idStr = File.ReadAllText(UserIdPath).Trim();
                    if (int.TryParse(idStr, out int id))
                    {
                        return id;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"사용자 ID 로드 실패: {ex.Message}");
            }
            return 0;
        }

        // 현재 사용자 ID 파일에 저장
        private static void SaveCurrentUserId(int userId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[SaveCurrentUserId] 저장 경로: {UserIdPath}");

                var directory = Path.GetDirectoryName(UserIdPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    System.Diagnostics.Debug.WriteLine($"[SaveCurrentUserId] 디렉토리 생성: {directory}");
                }

                File.WriteAllText(UserIdPath, userId.ToString());
                System.Diagnostics.Debug.WriteLine($"[SaveCurrentUserId] 저장 완료: UserId={userId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SaveCurrentUserId] 저장 실패: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[SaveCurrentUserId] 상세: {ex}");
            }
        }
    }
}
