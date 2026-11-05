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

        public int TotalQty { get; set; }
        public bool IsLowStock { get; set; }
        public string QuantityDisplay { get; set; }
    }

    public partial class ProductPage : Page
    {
        private List<ProductDisplay> _allProducts = new List<ProductDisplay>();

        public ProductPage()
        {
            InitializeComponent();
            FIOTB.Text = ManagerClass.CurrentUser != null ? ManagerClass.CurrentUser.UserSurname : "Гость";

            var cats = Kuzmin_ChudoObuvEntities.GetContext().Categories.ToList();
            cats.Insert(0, new Categories { ID_Category = 0, CategoryName = "Все категории" });
            ComboCategory.ItemsSource = cats;
            ComboCategory.DisplayMemberPath = "CategoryName";
            ComboCategory.SelectedIndex = 0;
            ComboSort.SelectedIndex = 0;
            if (ManagerClass.CurrentUser != null && (ManagerClass.CurrentUser.ID_Role == 1 || ManagerClass.CurrentUser.ID_Role == 2))
            {
                // Показываем кнопку "Все заказы"
                OrdersBtn.Visibility = Visibility.Visible;
            }
            else
            {
                // Скрываем для гостей и обычных пользователей
                OrdersBtn.Visibility = Visibility.Collapsed;
            }
            LoadData();
        }

        private void LoadData()
        {
            var ctx = Kuzmin_ChudoObuvEntities.GetContext();
            var products = ctx.Products.Include("Manufactures").Include("Categories").Include("StockItems").ToList();

            DateTime firstDayLastMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-1);
            DateTime lastDayLastMonth = firstDayLastMonth.AddMonths(1).AddDays(-1);

            // Получаем список ID товаров, которые заказывали в прошлом месяце (через SQL, чтобы не зависеть от связей EF)
            string sql = @"
        SELECT DISTINCT s.ID_Product 
        FROM OrderProducts op
        JOIN Orders o ON o.ID_Order = op.ID_Order
        JOIN StockItems s ON s.ID_Item = op.ID_Item 
        WHERE o.OrderDate >= @p0 AND o.OrderDate <= @p1";

            var orderedProductIds = ctx.Database
                .SqlQuery<int>(sql, firstDayLastMonth, lastDayLastMonth)
                .ToList();

            _allProducts = products.Select(p =>
            {
                int qty = p.StockItems != null ? p.StockItems.Sum(s => s.ItemsQuantity) : 0;

                // Есть ли этот товар в списке заказанных в прошлом месяце?
                bool hasOrdersLastMonth = orderedProductIds.Contains(p.ID_Product);

                // Скидка 25%, если его НЕ заказывали
                decimal price = p.ProductCost;
                if (!hasOrdersLastMonth)
                    price = price - (price * 0.25m);

                return new ProductDisplay
                {
                    ID_Product = p.ID_Product,
                    ProductName = p.ProductName,
                    ProductComposition = p.ProductComposition,
                    ProductDescription = p.ProductDescription,
                    ImagePath = p.ProductImage,
                    DiscountPrice = price,
                    ManufactureName = p.Manufactures != null ? p.Manufactures.ManufactureName : "",
                    CategoryName = p.Categories != null ? p.Categories.CategoryName : "",
                    TotalQty = qty,
                    IsLowStock = qty <= 3,
                    QuantityDisplay = qty > 5 ? "много" : "мало"
                };
            }).ToList();

            UpdateProduct();
        }

        private void UpdateProduct()
        {
            var displayList = _allProducts.ToList();

            var cat = ComboCategory.SelectedItem as Categories;
            if (cat != null && cat.ID_Category != 0)
                displayList = displayList.Where(p => p.CategoryName == cat.CategoryName).ToList();

            string search = TBoxSearch.Text.Trim().ToLower();
            if (search.Length > 0)
            {
                displayList = displayList.Where(p =>
                    (p.ProductName != null && p.ProductName.ToLower().Contains(search)) ||
                    (p.ProductDescription != null && p.ProductDescription.ToLower().Contains(search)) ||
                    (p.ManufactureName != null && p.ManufactureName.ToLower().Contains(search))
                ).ToList();
            }

            if (ComboSort.SelectedIndex == 1)
                displayList = displayList.OrderBy(p => p.DiscountPrice).ToList();
            else if (ComboSort.SelectedIndex == 2)
                displayList = displayList.OrderByDescending(p => p.DiscountPrice).ToList();

            ProductListView.ItemsSource = displayList;
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) { UpdateProduct(); }
        private void ComboCategory_SelectionChanged(object sender, SelectionChangedEventArgs e) { UpdateProduct(); }
        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e) { UpdateProduct(); }
        private void CartBtn_Click(object sender, RoutedEventArgs e) { ManagerClass.MainFrame.Navigate(new CartPage()); }

        private void ProductListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selected = ProductListView.SelectedItem as ProductDisplay;
            if (selected != null)
            {
                var product = Kuzmin_ChudoObuvEntities.GetContext().Products.FirstOrDefault(x => x.ID_Product == selected.ID_Product);
                if (product != null) ManagerClass.MainFrame.Navigate(new ProductDetailPage(product));
            }
        }

        private void OrdersBtn_Click(object sender, RoutedEventArgs e)
        {
            ManagerClass.MainFrame.Navigate(new OrdersPage());
        }
    }
}