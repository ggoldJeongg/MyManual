using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using MyManual.Services.Interfaces;
using MyManual.ViewModels;

namespace MyManual.Views
{
    public partial class ManualCreateView : UserControl
    {
        private INavigationService Navigation => App.Services.GetRequiredService<INavigationService>();

        public ManualCreateView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ManualCreateViewModel oldVm)
            {
                oldVm.ImageSelectRequested -= OnImageSelectRequested;
            }

            if (e.NewValue is ManualCreateViewModel newVm)
            {
                newVm.ImageSelectRequested += OnImageSelectRequested;
            }
        }

        private Task<List<string>> OnImageSelectRequested()
        {
            var dialog = new OpenFileDialog
            {
                Title = "이미지 파일 선택",
                Filter = "이미지 파일 (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|모든 파일 (*.*)|*.*",
                Multiselect = true
            };

            var result = new List<string>();

            if (dialog.ShowDialog() == true)
            {
                result.AddRange(dialog.FileNames);
            }

            return Task.FromResult(result);
        }

        private void OnManualButtonClick(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateToManual();
        }

        private void OnOnboardingButtonClick(object sender, RoutedEventArgs e)
        {
            Navigation.NavigateToOnboarding();
        }
    }
}
