using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using MyManual.Models.User;

namespace MyManual
{
    public partial class App : Application
    {
        // 앱 전역에서 사용할 현재 사용자
        public static User? CurrentUser { get; private set; }

        // 사용자 데이터 저장 경로
        private static readonly string UserDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MyManual",
            "user.json"
        );

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 저장된 사용자 정보 로드 시도
            CurrentUser = LoadUser();

            // MainWindow 생성 및 표시
            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();
        }

        // 사용자 정보 설정 및 저장
        public static void SetCurrentUser(User user)
        {
            CurrentUser = user;
            SaveUser(user);
        }

        // 사용자 정보 파일에서 로드
        private static User? LoadUser()
        {
            try
            {
                if (File.Exists(UserDataPath))
                {
                    var json = File.ReadAllText(UserDataPath);
                    return JsonSerializer.Deserialize<User>(json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"사용자 데이터 로드 실패: {ex.Message}");
            }
            return null;
        }

        // 사용자 정보 파일에 저장
        private static void SaveUser(User user)
        {
            try
            {
                var directory = Path.GetDirectoryName(UserDataPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(user, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(UserDataPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"사용자 데이터 저장 실패: {ex.Message}");
            }
        }
    }
}
