using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace KuzminChudoObuv
{
    public static class ManagerClass
    {
        public static Frame MainFrame { get; set; }
        public static Users CurrentUser { get; set; }
        public static List<OrderProducts> Cart = new List<OrderProducts>(); // <--- OrderProducts

        public static bool Checkout()
        {
            if (Cart.Count == 0 || CurrentUser == null) return false;
            var ctx = new Kuzmin_ChudoObuvEntities();
            try
            {
                // Считаем сумму
                decimal totalSum = 0;
                foreach (var item in Cart) totalSum += item.Quantity * item.PriceAtOrderTime;

                Orders newOrder = new Orders { OrderDate = System.DateTime.Now, ID_User = CurrentUser.ID_User, TotalAmount = totalSum };
                ctx.Orders.Add(newOrder);
                ctx.SaveChanges();

                foreach (var item in Cart)
                {
                    // Используем правильные названия полей из базы
                    OrderProducts op = new OrderProducts { ID_Order = newOrder.ID_Order, ID_StockItem = item.ID_StockItem, Quantity = item.Quantity, PriceAtOrderTime = item.PriceAtOrderTime };
                    ctx.OrderProducts.Add(op);

                    var stock = ctx.StockItems.FirstOrDefault(s => s.ID_StockItem == item.ID_StockItem);
                    if (stock != null) stock.ItemsQuantity -= item.Quantity;
                }
                ctx.SaveChanges();
                Cart.Clear();
                return true;
            }
            catch { return false; }
        }
    }
}