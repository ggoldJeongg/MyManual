using System;                  
using System.Windows;
using MyManual.Models.User;
using MyManual.ViewModels;
using MyManual.Views;

namespace MyManual
{
    public partial class App : Application
    {
protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var tempUser = new User
            {
                Id = 1,
                Name = "Jain",
                JoinDate = new DateTime(2026, 1, 2)
            };

            // MainWindow의 OnboardingView에 DataContext 설정
            var mainWindow = new MainWindow();
            var onboardingView = mainWindow.FindName("onboardingView") as OnboardingView;
            if (onboardingView != null)
            {
                onboardingView.DataContext = new OnboardingViewModel(tempUser);
            }
            
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}
