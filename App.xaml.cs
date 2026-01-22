using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyManual.Data;
using MyManual.Exceptions;
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

            // 전역 예외 핸들러 등록
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            try
            {
                // 0. 환경 변수 로드 (.env 파일)
                LoadEnvironmentVariables();

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
            catch (Exception ex)
            {
                ExceptionHandler.Handle(ex, "애플리케이션 시작");
                Shutdown(1);
            }
        }

        // UI 스레드 예외 처리
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ExceptionHandler.Handle(e.Exception, "예기치 않은 오류");
            e.Handled = true; // 예외 처리됨 표시 (앱 크래시 방지)
        }

        // 비-UI 스레드 예외 처리
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                // Dispatcher를 통해 UI 스레드에서 처리
                Dispatcher.Invoke(() => ExceptionHandler.Handle(ex, "치명적 오류"));
            }
        }

        /// <summary>
        /// .env 파일에서 환경 변수를 로드합니다.
        /// 우선순위: .env.local > .env (local 파일이 있으면 덮어씀)
        /// </summary>
        private void LoadEnvironmentVariables()
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var projectPath = Directory.GetCurrentDirectory();

            // 1. 기본 .env 파일 로드
            var envLoaded = TryLoadEnvFile(Path.Combine(basePath, ".env"))
                         || TryLoadEnvFile(Path.Combine(projectPath, ".env"));

            // 2. .env.local 파일이 있으면 덮어쓰기 (로컬 개발 환경용)
            var localLoaded = TryLoadEnvFile(Path.Combine(basePath, ".env.local"))
                           || TryLoadEnvFile(Path.Combine(projectPath, ".env.local"));

            if (localLoaded)
            {
                System.Diagnostics.Debug.WriteLine("[ENV] .env.local 파일로 설정이 덮어써졌습니다.");
            }
            else if (!envLoaded)
            {
                System.Diagnostics.Debug.WriteLine("[ENV] .env 파일을 찾을 수 없습니다. 기본값을 사용합니다.");
            }

            // Connection String 구성
            var server = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
            var database = Environment.GetEnvironmentVariable("DB_NAME") ?? "MyManual";
            var userId = Environment.GetEnvironmentVariable("DB_USER") ?? "";
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

            // Windows 인증 vs SQL Server 인증 분기
            if (string.IsNullOrEmpty(userId))
            {
                // Windows 인증 (LocalDB 등)
                AppDbContext.ConnectionString = $"Server={server};Database={database};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;";
                System.Diagnostics.Debug.WriteLine($"[ENV] Windows 인증 사용 - 서버: {server}, DB: {database}");
            }
            else
            {
                // SQL Server 인증
                AppDbContext.ConnectionString = $"Server={server};Database={database};User Id={userId};Password={password};TrustServerCertificate=True;MultipleActiveResultSets=True;";
                System.Diagnostics.Debug.WriteLine($"[ENV] SQL Server 인증 사용 - 서버: {server}, DB: {database}");
            }
        }

        /// <summary>
        /// .env 파일 로드를 시도합니다.
        /// </summary>
        private bool TryLoadEnvFile(string path)
        {
            if (File.Exists(path))
            {
                Env.Load(path);
                System.Diagnostics.Debug.WriteLine($"[ENV] 파일 로드: {path}");
                return true;
            }
            return false;
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // DbContext 등록 (Transient: 매 요청마다 새 인스턴스)
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(AppDbContext.ConnectionString);
            }, ServiceLifetime.Transient);

            // Services 등록
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IManualService, ManualService>();
            services.AddTransient<IOnboardingService, OnboardingService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddTransient<DatabaseInitializer>();

            // ImageService 등록 (Azure Blob Storage)
            var azureStorageConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING") ?? "";
            var azureContainerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER_NAME") ?? "manual-images";
            if (!string.IsNullOrEmpty(azureStorageConnectionString))
            {
                services.AddSingleton<IImageService>(provider =>
                    new ImageService(azureStorageConnectionString, azureContainerName));
                System.Diagnostics.Debug.WriteLine("[DI] ImageService 등록 완료");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[DI] AZURE_STORAGE_CONNECTION_STRING이 설정되지 않아 ImageService를 등록하지 않습니다.");
            }

            // MainWindow 등록
            services.AddTransient<MainWindow>();

            Services = services.BuildServiceProvider();
        }

        // 사용자 정보 설정 (이미 DB에서 검증된 User 객체를 받아 현재 사용자로 설정)
        public static void SetCurrentUser(User user)
        {
            // UserInitViewModel에서 로그인/회원가입 완료 후 DB의 User 객체를 전달받음
            CurrentUser = user;
            System.Diagnostics.Debug.WriteLine($"[SetCurrentUser] 사용자 설정: Id={CurrentUser.Id}, Name={CurrentUser.Name}, IsAdmin={CurrentUser.IsAdmin}");

            // 현재 사용자 ID만 파일에 저장
            SaveCurrentUserId(CurrentUser.Id);
        }

        // 로그아웃 (사용자 정보 초기화)
        public static void ClearCurrentUser()
        {
            CurrentUser = null;
            SaveCurrentUserId(0);
            System.Diagnostics.Debug.WriteLine("[ClearCurrentUser] 사용자 정보 초기화");
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
