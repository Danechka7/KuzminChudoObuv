using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KuzminChudoObuv
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
        }
        private void EnterBtn_Click(object sender, RoutedEventArgs e)
        {
            var user = KuzminChudoObuvEntities.GetContext().Users
                .FirstOrDefault(u => u.UserLogin == LoginBox.Text.Trim());

            if (user == null)
            {
                MessageBox.Show("Пользователь не найден");
                return;
            }

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
