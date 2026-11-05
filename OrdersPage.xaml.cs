using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KuzminChudoObuv
{
    // Вспомогательный класс для красивого вывода списка
    public class OrderDisplayItem
    {
        public int ID_Order { get; set; }
        public DateTime OrderDate { get; set; }
        public string ClientFIO { get; set; }
        public string TotalSumText { get; set; }
        public Orders SourceOrder { get; set; }
    }

    public partial class OrdersPage : Page
    {
        public OrdersPage()
        {
            InitializeComponent();

            // Скрываем кнопку удаления для обычных пользователей
            if (ManagerClass.CurrentUser == null || (ManagerClass.CurrentUser.ID_Role != 1 && ManagerClass.CurrentUser.ID_Role != 2))
            {
                DeleteBtn.Visibility = Visibility.Collapsed;
            }

            LoadOrders();
        }

        private void LoadOrders()
        {
            var ctx = Kuzmin_ChudoObuvEntities.GetContext();
            // Обязательно Include("Users"), чтобы подтянулись имена клиентов
            var orders = ctx.Orders.Include("Users").ToList();
            var displayList = new List<OrderDisplayItem>();

            foreach (var o in orders)
            {
                // Формируем ФИО
                string fio = "Неизвестно";
                if (o.Users != null)
                {
                    fio = o.Users.UserSurname + " " + o.Users.UserName + " " + o.Users.UserPatronymic;
                }

                // Формируем сумму (если TotalAmount пустой, ставим 0)
                decimal sum = o.TotalAmount ?? 0;

                displayList.Add(new OrderDisplayItem
                {
                    ID_Order = o.ID_Order,
                    OrderDate = o.OrderDate,
                    ClientFIO = fio,
                    TotalSumText = sum.ToString("N2") + " ₽",
                    SourceOrder = o
                });
            }

            OrdersListView.ItemsSource = displayList;
        }

        private void OrdersListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = OrdersListView.SelectedItem as OrderDisplayItem;
            if (selected != null)
            {
                ManagerClass.MainFrame.Navigate(new OrderDetailsPage(selected.SourceOrder));
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selected = OrdersListView.SelectedItem as OrderDisplayItem;
            if (selected == null)
            {
                MessageBox.Show("Выберите заказ для удаления.", "Ошибка");
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить заказ №{selected.ID_Order}?",
                                         "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            var ctx = Kuzmin_ChudoObuvEntities.GetContext();
            try
            {
                // Удаляем через SQL, чтобы не было ошибок EF
                string sqlDeleteItems = "DELETE FROM OrderProducts WHERE ID_Order = @p0";
                ctx.Database.ExecuteSqlCommand(sqlDeleteItems, selected.ID_Order);

                string sqlDeleteOrder = "DELETE FROM Orders WHERE ID_Order = @p0";
                ctx.Database.ExecuteSqlCommand(sqlDeleteOrder, selected.ID_Order);

                MessageBox.Show("Заказ успешно удален.", "Успех");
                LoadOrders(); // Обновляем таблицу
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении: " + ex.Message);
            }
        }
    }
}