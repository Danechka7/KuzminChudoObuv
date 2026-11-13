using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KuzminChudoObuv
{
    public class ProductDisplay
    {
        public int ID_Product { get; set; }
        public string ProductName { get; set; }
        public string ProductComposition { get; set; }
        public string ProductDescription { get; set; }
        public string ImagePath { get; set; }
        public decimal DiscountPrice { get; set; }
        public string ManufactureName { get; set; }
        public string CategoryName { get; set; }
        public bool IsLowStock { get; set; }
        public string QuantityDisplay { get; set; }
        public int TotalQty { get; set; }
    }
    public partial class ProductPage : Page
    {
        private List<ProductDisplay> allProducts = new List<ProductDisplay>();
        public ProductPage()
        {
            InitializeComponent();
            LoadData();
        }
        private void LoadData()
        {
            var ctx = new Kuzmin_ChudoObuvEntities();
            if (ManagerClass.CurrentUser != null)
            {
                string fio = ManagerClass.CurrentUser.UserSurname + " " +
                             ManagerClass.CurrentUser.UserName + " " +
                             (ManagerClass.CurrentUser.UserPatronymic ?? "");

                FIOTB.Text = "Вы вошли как: " + fio;
            }
            else
            {
                FIOTB.Text = "Гость";
            }
            var cats = ctx.Categories.ToList();
            cats.Insert(0, new Categories { ID_Category = 0, CategoryName = "Все категории" });
            ComboCategory.ItemsSource = cats;
            ComboCategory.DisplayMemberPath = "CategoryName";
            ComboCategory.SelectedIndex = 0;
            ComboSort.SelectedIndex = 0;
            if (ManagerClass.CurrentUser != null && (ManagerClass.CurrentUser.ID_Role == 1 || ManagerClass.CurrentUser.ID_Role == 2))
                OrdersBtn.Visibility = Visibility.Visible;
            else
                OrdersBtn.Visibility = Visibility.Collapsed;

            if (ManagerClass.CurrentUser != null && ManagerClass.CurrentUser.ID_Role == 1)
                AnalyticsBtn.Visibility = Visibility.Visible;
            else
                AnalyticsBtn.Visibility = Visibility.Collapsed;
            var products = ctx.Products.Include("Manufactures").Include("Categories").Include("StockItems").ToList();
            DateTime startDate = DateTime.Today.AddDays(-30);
            DateTime endDate = DateTime.Today.AddDays(1);
            var recentOrders = ctx.Orders.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate).ToList();
            List<int> orderedProductIds = new List<int>();
            foreach (var order in recentOrders)
                foreach (var op in order.OrderProducts)
                    if (!orderedProductIds.Contains(op.StockItems.ID_Product)) orderedProductIds.Add(op.StockItems.ID_Product);
            allProducts.Clear();
            foreach (var p in products)
            {
                ProductDisplay display = new ProductDisplay();
                display.ID_Product = p.ID_Product;
                display.ProductName = p.ProductName;
                display.ProductComposition = p.ProductComposition;
                display.ProductDescription = p.ProductDescription;
                display.ImagePath = p.ImagePath;
                display.ManufactureName = p.Manufactures != null ? p.Manufactures.ManufactureName : "";
                display.CategoryName = p.Categories != null ? p.Categories.CategoryName : "";
                int qty = 0;
                if (p.StockItems != null) foreach (var s in p.StockItems) qty += s.ItemsQuantity;
                display.TotalQty = qty;
                display.IsLowStock = qty <= 3;
                display.QuantityDisplay = qty > 5 ? "много" : "мало";
                display.DiscountPrice = orderedProductIds.Contains(p.ID_Product) ? p.ProductCost : p.ProductCost - (p.ProductCost * 0.25m);
                allProducts.Add(display);
            }
            UpdateProduct();
        }
        private void UpdateProduct()
        {
            List<ProductDisplay> filtered = new List<ProductDisplay>();
            Categories cat = ComboCategory.SelectedItem as Categories;
            int catId = cat != null ? cat.ID_Category : 0;
            string search = TBoxSearch.Text.Trim().ToLower();

            foreach (var p in allProducts)
            {
                bool keep = true;
                if (catId != 0 && p.CategoryName != cat.CategoryName)
                    keep = false;
                if (search.Length > 0)
                {
                    bool nameMatch = p.ProductName != null && p.ProductName.ToLower().Contains(search);
                    bool manufMatch = p.ManufactureName != null && p.ManufactureName.ToLower().Contains(search);
                    if (!nameMatch && !manufMatch) keep = false;
                }
                if (keep) filtered.Add(p);
            }
            if (ComboSort.SelectedIndex == 1)
                filtered = filtered.OrderBy(p => p.DiscountPrice).ToList();
            else if (ComboSort.SelectedIndex == 2)
                filtered = filtered.OrderByDescending(p => p.DiscountPrice).ToList();
            ProductListView.ItemsSource = filtered;
        }
        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) { UpdateProduct(); }
        private void ComboCategory_SelectionChanged(object sender, SelectionChangedEventArgs e) { UpdateProduct(); }
        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e) { UpdateProduct(); }
        private void CartBtn_Click(object sender, RoutedEventArgs e) { ManagerClass.MainFrame.Navigate(new CartPage()); }
        private void OrdersBtn_Click(object sender, RoutedEventArgs e) { ManagerClass.MainFrame.Navigate(new OrdersPage()); }
        private void AnalyticsBtn_Click(object sender, RoutedEventArgs e) { ManagerClass.MainFrame.Navigate(new AnalyticsPage()); }
        private void ProductListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ProductDisplay selected = ProductListView.SelectedItem as ProductDisplay;
            if (selected != null)
            {
                var ctx = new Kuzmin_ChudoObuvEntities();
                var product = ctx.Products.FirstOrDefault(x => x.ID_Product == selected.ID_Product);
                if (product != null) ManagerClass.MainFrame.Navigate(new ProductDetailPage(product));
            }
        }
    }
}