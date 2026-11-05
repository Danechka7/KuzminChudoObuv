using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KuzminChudoObuv
{
    public partial class AuthPage : Page
    {
        public AuthPage() { InitializeComponent(); }

        private void GuestBtn_Click(object sender, RoutedEventArgs e)
        {
            ManagerClass.CurrentUser = null;
            ManagerClass.MainFrame.Navigate(new ProductPage());
        }
        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text;
            if (login == "") { MessageBox.Show("Введите логин!"); return; }
            Users user = Kuzmin_ChudoObuvEntities.GetContext().Users.ToList().Find(p => p.UserLogin == login);
            if (user != null)
            {
                ManagerClass.CurrentUser = user;
                ManagerClass.MainFrame.Navigate(new ProductPage());
            }
            else MessageBox.Show("Неверный логин!");
        }
    }
}