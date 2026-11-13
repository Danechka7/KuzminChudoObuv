using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KuzminChudoObuv
{
    public class SizeItem
    {
        public int ID_StockItem { get; set; }
        public string SizeText { get; set; }
    }
    public partial class ProductDetailPage : Page
    {
        private Products currentProduct;
        private int quantity = 1;
        public ProductDetailPage(Products product)
        {
            InitializeComponent();
            currentProduct = product;
            var ctx = new Kuzmin_ChudoObuvEntities();
            DateTime startDate = DateTime.Today.AddDays(-30);
            DateTime endDate = DateTime.Today.AddDays(1);
            bool hasRecentOrders = false;

            var recentOrders = ctx.Orders.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate).ToList();
            foreach (var order in recentOrders)
            {
                foreach (var op in order.OrderProducts)
                {
                    if (op.StockItems.ID_Product == product.ID_Product) hasRecentOrders = true;
                }
            }
            if (hasRecentOrders)
                currentProduct.DiscountPrice = currentProduct.ProductCost;
            else
                currentProduct.DiscountPrice = currentProduct.ProductCost - (currentProduct.ProductCost * 0.25m);

            DataContext = currentProduct;
            var sizes = ctx.StockItems.Where(s => s.ID_Product == product.ID_Product && s.ItemsQuantity > 0).ToList();
            ComboSizes.ItemsSource = sizes;
            ComboSizes.DisplayMemberPath = "Sizes.Size";
            if (sizes.Count > 0) ComboSizes.SelectedIndex = 0;
        }
        private void Plus_Click(object sender, RoutedEventArgs e) { quantity++; TxtQty.Text = quantity.ToString(); }
        private void Minus_Click(object sender, RoutedEventArgs e) { if (quantity > 1) { quantity--; TxtQty.Text = quantity.ToString(); } }
        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            StockItems selectedStock = ComboSizes.SelectedItem as StockItems;
            if (selectedStock == null) { MessageBox.Show("Выберите размер!"); return; }

            if (quantity > selectedStock.ItemsQuantity) { MessageBox.Show("На складе не хватает товара!"); return; }
            var existing = ManagerClass.Cart.FirstOrDefault(c => c.ID_StockItem == selectedStock.ID_StockItem);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                OrderProducts newItem = new OrderProducts();
                newItem.ID_StockItem = selectedStock.ID_StockItem;
                newItem.Quantity = quantity;
                newItem.PriceAtOrderTime = currentProduct.DiscountPrice;

                ManagerClass.Cart.Add(newItem);
            }

            MessageBox.Show("Добавлено в корзину!");
            ManagerClass.MainFrame.GoBack();
        }
    }
}