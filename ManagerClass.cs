using System;
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
        public static void AddToCart(int stockItemId, string productName, string sizeText, int qty, decimal price)
        {
            var existing = Cart.FirstOrDefault(c => c.StockItemID == stockItemId);
            if (existing != null)
            {
                existing.Quantity += qty;
            }
            else
            {
                Cart.Add(new CartItem
                {
                    StockItemID = stockItemId,
                    ProductName = productName,
                    SizeText = sizeText,
                    Quantity = qty,
                    Price = price
                });
            }
        }
        public static bool Checkout()
        {
            if (Cart.Count == 0) return false;
            if (CurrentUser == null) return false;

            var ctx = KuzminChudoObuvEntities.GetContext();

            try
            {
                var newOrder = new Orders
                {
                    OrderDate = DateTime.Now,
                    ID_User = CurrentUser.ID_User
                };

                ctx.Orders.Add(newOrder);
                ctx.SaveChanges();
                foreach (var item in Cart)
                {
                    var newOP = new OrderProducts
                    {
                        ID_Order = newOrder.ID_Order,
                        Quantity = item.Quantity
                    };
                    ctx.OrderProducts.Add(newOP);
                    var stock = ctx.StockItems.FirstOrDefault(s => s.ID_Item == item.StockItemID);
                    if (stock != null)
                    {
                        stock.ItemsQuantity -= item.Quantity;
                        if (stock.ItemsQuantity < 0) stock.ItemsQuantity = 0;
                    }
                }

                ctx.SaveChanges();
                Cart.Clear();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
                return false;
            }
        }
    }
}