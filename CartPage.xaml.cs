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
            LoadCart();
        }

        private void LoadCart()
        {
            CartListView.ItemsSource = null;
            CartListView.ItemsSource = ManagerClass.Cart;
            TotalText.Text = "Итого: " + ManagerClass.Cart.Sum(c => c.Total).ToString("N2") + " ₽";
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var item = btn.Tag as CartItem;
            if (item != null)
            {
                ManagerClass.Cart.Remove(item);
                LoadCart();
            }
        }

        private void Plus_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var item = btn.Tag as CartItem;

            if (item != null)
            {
                var ctx = Kuzmin_ChudoObuvEntities.GetContext();
                var stock = ctx.StockItems.FirstOrDefault(s => s.ID_Item == item.StockItemID);

                if (stock != null && item.Quantity + 1 > stock.ItemsQuantity)
                {
                    MessageBox.Show($"На складе осталось только {stock.ItemsQuantity} шт.!");
                    return;
                }

                item.Quantity++;
                LoadCart();
            }
        }

        private void Minus_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var item = btn.Tag as CartItem;

            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
                LoadCart();
            }
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