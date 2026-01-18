using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using MyManual.Services.Interfaces;

namespace MyManual.Views
{
    public partial class CategoryMenuView : UserControl
    {
        private INavigationService Navigation => App.Services.GetRequiredService<INavigationService>();

        public CategoryMenuView()
        {
            InitializeComponent();
        }

        // ==================== UI 레이아웃 (코드비하인드에서 처리해야 하는 영역) ====================

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGridLayout(e.NewSize.Width, e.NewSize.Height);
        }

        private void UpdateGridLayout(double width, double height)
        {
            var uniformGrid = FindUniformGrid(CategoryItemsControl);
            if (uniformGrid == null) return;

            if (width >= 1200 && height >= 600)
            {
                uniformGrid.Columns = 2;
                uniformGrid.Rows = 2;
            }
            else if (width >= 1000)
            {
                uniformGrid.Columns = 4;
                uniformGrid.Rows = 1;
            }
            else if (width >= 700)
            {
                uniformGrid.Columns = 2;
                uniformGrid.Rows = 2;
            }
            else
            {
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

        // ==================== 네비게이션 (Header 이벤트) ====================

        private void OnOnboardingButtonClick(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateToOnboarding();
        }

        private void OnManualButtonClick(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateToManual();
        }

        private void OnOnboardingManageClick(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateToOnboardingManage();
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
