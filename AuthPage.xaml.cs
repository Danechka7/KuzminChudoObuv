using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KuzminChudoObuv
{
    public partial class AuthPage : Page
    {
        public AuthPage() { InitializeComponent(); }

        private void EnterBtn_Click(object sender, RoutedEventArgs e)
        {
            var user = KuzminChudoObuvEntities.GetContext().Users.FirstOrDefault(u => u.UserLogin == LoginBox.Text.Trim());
            if (user == null) { MessageBox.Show("Пользователь не найден"); return; }
            ManagerClass.CurrentUser = user;
            ManagerClass.MainFrame.Navigate(new ProductPage());
        }

        private void GuestBtn_Click(object sender, RoutedEventArgs e)
        {
            ManagerClass.CurrentUser = null;
            ManagerClass.MainFrame.Navigate(new ProductPage());
        }
    }
}