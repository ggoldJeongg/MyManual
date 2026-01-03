using System.Windows;
using OnboardingManual.ViewModels;
using OnboardingManual.Views;

namespace OnboardingManual
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var view = new OnboardingView
            {
                DataContext = new OnboardingViewModel()
            };

            var window = new Window
            {
                Title = "OnboardingManual",
                Content = view,
                Width = 1280,
                Height = 1080,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            window.Show();
        }
    }
}
