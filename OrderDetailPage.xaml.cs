using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KuzminChudoObuv
{
    public partial class OrderDetailPage : Page
    {
        private Orders currentOrder;

        // Свойство для показа кнопок удаления только Админу
        public Visibility IsAdminVisible
        {
            get
            {
                if (ManagerClass.CurrentUser != null && ManagerClass.CurrentUser.ID_Role == 1)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public OrderDetailPage(Orders order)
        {
            InitializeComponent();
            currentOrder = order;

            // Ограничение: в календаре нельзя выбрать дату больше сегодняшней
            OrderDatePicker.DisplayDateEnd = DateTime.Today;

            var ctx = new Kuzmin_ChudoObuvEntities();
            var client = ctx.Users.FirstOrDefault(u => u.ID_User == order.ID_User);

            OrderInfoText.Text = "Заказ №" + order.ID_Order;
            ClientInfoText.Text = "Клиент: " + (client != null ? client.UserSurname + " " + client.UserName : "Неизвестен");

            OrderDatePicker.SelectedDate = order.OrderDate;

            if (ManagerClass.CurrentUser != null && ManagerClass.CurrentUser.ID_Role == 1)
                EditPanel.Visibility = Visibility.Visible;

            LoadItems();
        }

        private void LoadItems()
        {
            var ctx = new Kuzmin_ChudoObuvEntities();

            var items = ctx.OrderProducts
                .Include("StockItems.Products")
                .Include("StockItems.Sizes")
                .Where(op => op.ID_Order == currentOrder.ID_Order)
                .ToList();

            OrderItemsListView.ItemsSource = items;

            decimal total = 0;
            foreach (var item in items) total += item.Quantity * item.PriceAtOrderTime;
            TotalText.Text = "Итого: " + total.ToString("N2") + " ₽";
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (ManagerClass.CurrentUser == null || ManagerClass.CurrentUser.ID_Role != 1) return;

            var btn = sender as Button;
            var item = btn.Tag as OrderProducts;
            if (item == null) return;

            if (MessageBox.Show("Удалить эту позицию из заказа?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var ctx = new Kuzmin_ChudoObuvEntities();

                var stock = ctx.StockItems.FirstOrDefault(s => s.ID_StockItem == item.ID_StockItem);
                if (stock != null) stock.ItemsQuantity += item.Quantity;

                string sqlDelete = "DELETE FROM OrderProducts WHERE ID_OrderItem = @p0";
                ctx.Database.ExecuteSqlCommand(sqlDelete, item.ID_OrderItem);

                ctx.SaveChanges();
                LoadItems(); // Обновляем таблицу сразу
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (ManagerClass.CurrentUser == null || ManagerClass.CurrentUser.ID_Role != 1) return;
            if (OrderDatePicker.SelectedDate == null) { MessageBox.Show("Выберите дату!"); return; }

            DateTime newDate = OrderDatePicker.SelectedDate.Value.Date;

            // Проверка, чтобы не поставить будущую дату
            if (newDate > DateTime.Today)
            {
                MessageBox.Show("Дата заказа не может быть в будущем!", "Ошибка");
                OrderDatePicker.SelectedDate = currentOrder.OrderDate;
                return;
            }

            var ctx = new Kuzmin_ChudoObuvEntities();
            var dbOrder = ctx.Orders.FirstOrDefault(o => o.ID_Order == currentOrder.ID_Order);

            if (dbOrder != null)
            {
                dbOrder.OrderDate = newDate;
                ctx.SaveChanges();

                // Вместо MessageBox просто переходим обратно на страницу заказов
                // ManagerClass.MainFrame.GoBack(); // Если хочешь вернуться назад
                ManagerClass.MainFrame.Navigate(new OrdersPage()); // Если хочешь обновить страницу заказов заново
            }
        }
    }
}