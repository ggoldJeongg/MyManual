using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MyManual.ViewModels;

namespace MyManual.Views
{
    public partial class CategoryMenuView : UserControl
    {
        public CategoryMenuView()
        {
            InitializeComponent();
        }

        private void OnOnboardingButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToOnboarding();
        }

        private void OnManualButtonClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToManual();
        }

        private void OnCategoryHeaderClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is CategoryGroup group &&
                DataContext is CategoryMenuViewModel vm)
            {
                vm.OnCategoryClick(group.CategoryName);
            }
        }

        private void OnManualItemClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is ManualSummary manual &&
                DataContext is CategoryMenuViewModel vm)
            {
                vm.OnManualClick(manual.Id);
            }
        }
    }
}
