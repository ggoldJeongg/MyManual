using System.Windows;
using OnboardingManual.ViewModels;

namespace OnboardingManual;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new OnboardingViewModel();
    }
}
