using System;
using System.Linq;
using System.Windows.Controls;

namespace KuzminChudoObuv
{
    public class OrderDetailsDisplay
    {
        public string ProductName { get; set; }
        public string SizeText { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalSum { get; set; }
    }

    public partial class OrderDetailsPage : Page
    {
        public OrderDetailsPage(Orders order)
        {
            InitializeComponent();

            OrderInfoText.Text = "Заказ №" + order.ID_Order + " от " + order.OrderDate.ToString("dd.MM.yyyy");
            ClientInfoText.Text = "Клиент: " + order.Users.UserSurname + " " + order.Users.UserName + " " + order.Users.UserPatronymic;

            var ctx = Kuzmin_ChudoObuvEntities.GetContext();

            // Подтягиваем позиции, товары и размеры
            var items = ctx.OrderProducts
                .Include("StockItems.Products")
                .Include("StockItems.Sizes")
                .Where(op => op.ID_Order == order.ID_Order)
                .ToList();

            var displayList = items.Select(op => new OrderDetailsDisplay
            {
                ProductName = op.StockItems.Products.ProductName,
                SizeText = op.StockItems.Sizes.Size.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Quantity = op.Quantity,
                // Берем текущую цену товара из базы
                Price = op.StockItems.Products.ProductCost,
                TotalSum = op.Quantity * op.StockItems.Products.ProductCost
            }).ToList();

            OrderItemsListView.ItemsSource = displayList;

            // Считаем общую сумму (можно взять из order.TotalAmount, но посчитаем для надежности)
            decimal total = displayList.Sum(x => x.TotalSum);
            TotalText.Text = "Итого: " + total.ToString("N2") + " ₽";
        }
    }
}