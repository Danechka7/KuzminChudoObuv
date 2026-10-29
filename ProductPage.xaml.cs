using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KuzminChudoObuv
{
    public partial class ProductPage : Page
    {
        public ProductPage()
        {
            InitializeComponent();
            FIOTB.Text = ManagerClass.CurrentUser != null ? ManagerClass.CurrentUser.UserSurname : "Гость";

            // Загрузка категорий
            var cats = KuzminChudoObuvEntities.GetContext().Categories.ToList();
            cats.Insert(0, new Categories { ID_Category = 0, CategoryName = "Все категории" });
            ComboCategory.ItemsSource = cats;
            ComboCategory.DisplayMemberPath = "CategoryName";
            ComboCategory.SelectedIndex = 0;

            UpdateProduct();
        }

        private void UpdateProduct()
        {
            var ctx = KuzminChudoObuvEntities.GetContext();

            // Загружаем товары с подгрузкой связанных таблиц
            var products = ctx.Products
                .Include("Manufactures")
                .Include("StockItems")
                .ToList();

            // Заполняем расчетные поля (количество, скидка, размеры)
            foreach (var p in products)
            {
                // 1. Считаем общее количество
                int totalQty = p.StockItems.Sum(s => s.ItemsQuantity);
                p.QuantityText = totalQty > 5 ? "много" : "мало";
                p.IsLowStock = totalQty <= 3; // Если <= 3 — будет подсветка красным

                // 2. Считаем цену со скидкой (пока просто как есть, без скидки)
                p.DiscountPriceText = p.ProductCost.ToString("0.00") + " ₽";

                // 3. Размеры (если нужно)
                var sizes = p.StockItems.Where(s => s.ItemsQuantity > 0)
                    .Select(s => s.Sizes.Size.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    .ToList();
                p.SizesText = "Размеры: " + string.Join(", ", sizes);
            }

            // Фильтр по категории
            var cat = ComboCategory.SelectedItem as Categories;
            if (cat != null && cat.ID_Category != 0)
            {
                products = products.Where(p => p.ID_Category == cat.ID_Category).ToList();
            }

            // Поиск по названию и описанию
            string search = TBoxSearch.Text.Trim().ToLower();
            if (search.Length > 0)
            {
                products = products.Where(p =>
                    (p.ProductName != null && p.ProductName.ToLower().Contains(search)) ||
                    (p.ProductDescription != null && p.ProductDescription.ToLower().Contains(search))
                ).ToList();
            }

            ProductListView.ItemsSource = products;
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) { UpdateProduct(); }
        private void ComboCategory_SelectionChanged(object sender, SelectionChangedEventArgs e) { UpdateProduct(); }

        private void CartBtn_Click(object sender, RoutedEventArgs e)
        {
            ManagerClass.MainFrame.Navigate(new CartPage());
        }

        // Переход на страницу товара по двойному клику
        private void ProductListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedProduct = ProductListView.SelectedItem as Products;
            if (selectedProduct != null)
            {
                ManagerClass.MainFrame.Navigate(new ProductDetailPage(selectedProduct));
            }
        }
    }
}