using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace KuzminChudoObuv
{
    public partial class OrdersPage : Page
    {
        public OrdersPage()
        {
            InitializeComponent();
            OrdersListView.ItemsSource = KuzminChudoObuvEntities.GetContext().Orders.Include("Users").ToList();
        }

        private void OrdersListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var order = OrdersListView.SelectedItem as Orders;
            if (order != null) ManagerClass.MainFrame.Navigate(new OrderDetailsPage(order));
        }
    }
}