using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KuzminChudoObuv
{
    public class CartItem
    {
        public int StockItemID { get; set; }
        public string ProductName { get; set; }
        public string SizeText { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get { return Quantity * Price; } }
    }

    public static class ManagerClass
    {
        public static Frame MainFrame { get; set; }
        public static Users CurrentUser { get; set; }
        public static List<CartItem> Cart = new List<CartItem>();
        public static void AddToCart(int id, string name, string size, int qty, decimal price)
        {
            var item = Cart.FirstOrDefault(c => c.StockItemID == id);
            if (item != null) item.Quantity += qty;
            else Cart.Add(new CartItem { StockItemID = id, ProductName = name, SizeText = size, Quantity = qty, Price = price });
        }
        public static bool Checkout()
        {
            if (Cart.Count == 0 || CurrentUser == null) return false;
            var ctx = new Kuzmin_ChudoObuvEntities();
            try
            {
                Orders newOrder = new Orders { OrderDate = System.DateTime.Now, ID_User = CurrentUser.ID_User, TotalAmount = Cart.Sum(c => c.Total) };
                ctx.Orders.Add(newOrder);
                ctx.SaveChanges();
                foreach (var item in Cart)
                {
                    OrderProducts op = new OrderProducts { ID_Order = newOrder.ID_Order, ID_StockItem = item.StockItemID, Quantity = item.Quantity, PriceAtOrderTime = item.Price };
                    ctx.OrderProducts.Add(op);
                    var stock = ctx.StockItems.FirstOrDefault(s => s.ID_StockItem == item.StockItemID);
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