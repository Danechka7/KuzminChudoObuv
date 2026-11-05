using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KuzminChudoObuv
{
    public partial class OrdersPage : Page
    {
        public OrdersPage()
        {
            InitializeComponent();
            LoadOrders();
        }
        private void LoadOrders()
        {
            var ctx = new Kuzmin_ChudoObuvEntities();
            var orders = ctx.Orders.Include("Users").ToList();
            List<OrderDisplayItem> displayList = new List<OrderDisplayItem>();
            foreach (var o in orders)
            {
                OrderDisplayItem item = new OrderDisplayItem();
                item.ID_Order = o.ID_Order;
                item.OrderDate = o.OrderDate;
                item.ClientFIO = o.Users != null ? o.Users.UserSurname + " " + o.Users.UserName + " " + o.Users.UserPatronymic : "Неизвестно";
                item.TotalSum = o.TotalAmount ?? 0;
                item.SourceOrder = o;
                displayList.Add(item);
            }
            OrderListView.ItemsSource = displayList;
        }
        private void OrderListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OrderDisplayItem selected = OrderListView.SelectedItem as OrderDisplayItem;
            if (selected != null)
            {
                ManagerClass.MainFrame.Navigate(new OrderDetailPage(selected.SourceOrder));
            }
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            OrderDisplayItem selected = OrderListView.SelectedItem as OrderDisplayItem;
            if (selected == null) { MessageBox.Show("Выберите заказ."); return; }
            if (MessageBox.Show("Удалить заказ?", "Вопрос", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
            var ctx = new Kuzmin_ChudoObuvEntities();
            try
            {
                string sqlDelItems = "DELETE FROM OrderProducts WHERE ID_Order = @p0";
                ctx.Database.ExecuteSqlCommand(sqlDelItems, selected.ID_Order);
                string sqlDelOrder = "DELETE FROM Orders WHERE ID_Order = @p0";
                ctx.Database.ExecuteSqlCommand(sqlDelOrder, selected.ID_Order);
                MessageBox.Show("Заказ удален.");
                ManagerClass.MainFrame.Navigate(new ProductPage());
            }
            catch (Exception ex) { MessageBox.Show("Ошибка: " + ex.Message); }
        }
        private void OrdersListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }
    }
    public class OrderDisplayItem
    {
        public int ID_Order { get; set; }
        public DateTime OrderDate { get; set; }
        public string ClientFIO { get; set; }
        public decimal TotalSum { get; set; }
        public Orders SourceOrder { get; set; }
    }
}