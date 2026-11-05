using System.Linq;
using System.Windows.Controls;

namespace KuzminChudoObuv
{
    public partial class OrderDetailPage : Page
    {
        public OrderDetailPage(Orders order)
        {
            InitializeComponent();
            OrderInfoText.Text = "Заказ №" + order.ID_Order + " от " + order.OrderDate.ToString("dd.MM.yyyy");
            var ctx = Kuzmin_ChudoObuvEntities.GetContext();
            var client = ctx.Users.FirstOrDefault(u => u.ID_User == order.ID_User);
            ClientInfoText.Text = "Клиент: " + (client != null ? client.UserSurname + " " + client.UserName : "Неизвестен");
            var items = ctx.OrderProducts.Include("StockItems.Products").Include("StockItems.Sizes").Where(op => op.ID_Order == order.ID_Order).ToList();
            OrderItemsListView.ItemsSource = items;
            decimal total = 0;
            foreach (var item in items) total += item.Quantity * item.PriceAtOrderTime;
            TotalText.Text = "Итого: " + total.ToString("N2") + " ₽";
        }
    }
}