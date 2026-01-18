using System.Windows.Controls;
using MyManual.ViewModels;

namespace MyManual.Views
{
    public partial class UserInitView : UserControl
    {
        public UserInitView()
        {
            InitializeComponent();
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
