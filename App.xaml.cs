using System;
using System.Windows;
using MyManual.Models.User;
using MyManual.ViewModels;
using MyManual.Views;

namespace MyManual
{
    public partial class App : Application
    {
        // 앱 전역에서 사용할 현재 사용자 (나중에 DB에서 로드)
        public static User? CurrentUser { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // TODO: 나중에 DB 또는 로그인에서 사용자 정보 로드
            // 지금은 임시 하드코딩 데이터
            CurrentUser = new User
            {
                Id = 1,
                Name = "김신입",
                JoinDate = new DateTime(2026, 1, 8)
            };

            // MainWindow 생성 및 표시
            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
