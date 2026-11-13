using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

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
            // Чтобы таблица отобразила названия и размеры, нам нужно подтянуть их в память
            var ctx = new Kuzmin_ChudoObuvEntities();

            foreach (var item in ManagerClass.Cart)
            {
                // Вручную подгружаем связанные данные для красивого вывода в таблице
                item.StockItems = ctx.StockItems.FirstOrDefault(s => s.ID_StockItem == item.ID_StockItem);
                if (item.StockItems != null)
                {
                    item.StockItems.Products = ctx.Products.FirstOrDefault(p => p.ID_Product == item.StockItems.ID_Product);
                    item.StockItems.Sizes = ctx.Sizes.FirstOrDefault(s => s.ID_Size == item.StockItems.ID_Size);
                }
            }

            // Передаем корзину в таблицу
            CartListView.ItemsSource = null;
            CartListView.ItemsSource = ManagerClass.Cart.ToList();

            // Считаем итог
            decimal total = 0;
            foreach (var item in ManagerClass.Cart)
            {
                total += item.Quantity * item.PriceAtOrderTime;
            }
            TotalText.Text = "Итого: " + total.ToString("N2") + " ₽";
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var item = btn.Tag as OrderProducts; // <--- ТЕПЕРЬ OrderProducts
            if (item != null)
            {
                ManagerClass.Cart.Remove(item);
                LoadCart();
            }
        }

        private void Plus_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var item = btn.Tag as OrderProducts; // <--- ТЕПЕРЬ OrderProducts
            if (item != null)
            {
                var ctx = new Kuzmin_ChudoObuvEntities();
                var stock = ctx.StockItems.FirstOrDefault(s => s.ID_StockItem == item.ID_StockItem);

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
            var item = btn.Tag as OrderProducts; // <--- ТЕПЕРЬ OrderProducts
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