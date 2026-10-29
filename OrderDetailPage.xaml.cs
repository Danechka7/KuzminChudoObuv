using System.Linq;
using System.Windows.Controls;

namespace KuzminChudoObuv
{
    public partial class OrderDetailsPage : Page
    {
        public OrderDetailsPage(Orders order)
        {
            InitializeComponent();
            OrderInfoText.Text = "Заказ №" + order.ID_Order + " от " + order.OrderDate.ToString("dd.MM.yyyy");
            ClientInfoText.Text = "Клиент: " + order.Users.UserSurname + " " + order.Users.UserName;
            var ctx = KuzminChudoObuvEntities.GetContext();
            ItemsListView.ItemsSource = ctx.OrderProducts.Where(op => op.ID_Order == order.ID_Order).ToList();
        }
    }
}