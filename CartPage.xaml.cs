using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KuzminChudoObuv
{
    public partial class CartPage : Page
    {
        public CartPage()
        {
            InitializeComponent();
            CartListView.ItemsSource = ManagerClass.Cart;
            TotalText.Text = "Итого: " + ManagerClass.Cart.Sum(c => c.Total).ToString("N2") + " ₽";
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (ManagerClass.Cart.Count == 0) { MessageBox.Show("Корзина пуста."); return; }
            if (ManagerClass.CurrentUser == null) { MessageBox.Show("Войдите в аккаунт!"); return; }

            if (ManagerClass.Checkout())
            {
                MessageBox.Show("Заказ оформлен!");
                ManagerClass.MainFrame.Navigate(new ProductPage());
            }
        }
    }
}