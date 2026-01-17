using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using MyManual.Services.Interfaces;
using MyManual.ViewModels;

namespace MyManual.Views
{
    public partial class CategoryMenuView : UserControl
    {
        private INavigationService Navigation => App.Services.GetRequiredService<INavigationService>();

        public CategoryMenuView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 화면 크기 변경 시 그리드 레이아웃 조정
        /// </summary>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGridLayout(e.NewSize.Width, e.NewSize.Height);
        }

        private void UpdateGridLayout(double width, double height)
        {
            var uniformGrid = FindUniformGrid(CategoryItemsControl);
            if (uniformGrid == null) return;

            // 화면 크기에 따라 그리드 레이아웃 결정
            // 카테고리 4개 기준
            if (width >= 1200 && height >= 600)
            {
                // 넓고 높음: 2x2
                uniformGrid.Columns = 2;
                uniformGrid.Rows = 2;
            }
            else if (width >= 1000)
            {
                // 넓음: 4x1
                uniformGrid.Columns = 4;
                uniformGrid.Rows = 1;
            }
            else if (width >= 700)
            {
                // 중간: 2x2
                uniformGrid.Columns = 2;
                uniformGrid.Rows = 2;
            }
            else
            {
                // 좁음: 1x4 (세로 스크롤 필요할 수 있음)
                uniformGrid.Columns = 1;
                uniformGrid.Rows = 4;
            }
        }

        private UniformGrid? FindUniformGrid(DependencyObject parent)
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is UniformGrid grid)
                    return grid;

                var result = FindUniformGrid(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void OnOnboardingButtonClick(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateToOnboarding();
        }

        private void OnManualButtonClick(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateToManual();
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

        private void OnUserNameClick(object sender, RoutedEventArgs e)
        {
            ProfileDrawer.Open();
        }

        private void OnLogoutRequested(object? sender, EventArgs e)
        {
            Navigation.NavigateToUserInit();
        }
    }
}
