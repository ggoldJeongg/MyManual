using System.Windows.Controls;
using System.Windows.Input;
using MyManual.ViewModels;

namespace MyManual.Views
{
    public partial class UserInitView : UserControl
    {
        public UserInitView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is UserInitViewModel oldVm)
            {
                oldVm.PasswordResetRequested -= ClearPasswordBoxes;
            }

            if (e.NewValue is UserInitViewModel newVm)
            {
                newVm.PasswordResetRequested += ClearPasswordBoxes;
            }
        }

        private void ClearPasswordBoxes()
        {
            PasswordBox.Clear();
            PasswordConfirmBox.Clear();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is UserInitViewModel vm)
            {
                if (vm.SubmitCommand.CanExecute(null))
                {
                    vm.SubmitCommand.Execute(null);
                }
            }
        }

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is UserInitViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        }

        private void PasswordConfirmBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is UserInitViewModel vm)
            {
                vm.PasswordConfirm = PasswordConfirmBox.Password;
            }
        }
    }
}
