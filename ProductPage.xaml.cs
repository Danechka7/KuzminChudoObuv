using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KuzminChudoObuv
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public ProductPage()
        {
            InitializeComponent();
            if (ManagerClass.CurrentUser != null)
            {
                FIOTB.Text = "Вы вошли как: " + ManagerClass.CurrentUser.UserSurname + " " +
                             ManagerClass.CurrentUser.UserName + " " +
                             ManagerClass.CurrentUser.UserPatronymic;
            }
            else
            {
                FIOTB.Text = "Вы вошли как: Гость";
            }

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
            var products = ctx.Products.ToList();
            var stock = ctx.StockItems.Include("Sizes").ToList();

            foreach (var p in products)
            {
                var sizes = stock
                    .Where(s => s.ID_Product == p.ID_Product && s.ItemsQuantity > 0)
                    .Select(s => s.Sizes.Size.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    .ToList();

                p.SizesText = "Размеры: " + string.Join(",     ", sizes);
            }

            var cat = ComboCategory.SelectedItem as Categories;
            if (cat != null && cat.ID_Category != 0)
                products = products.Where(p => p.ID_Category == cat.ID_Category).ToList();

            if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
                products = products.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            if (RButtonUp.IsChecked == true) products = products.OrderBy(p => p.ProductCost).ToList();
            if (RButtonDown.IsChecked == true) products = products.OrderByDescending(p => p.ProductCost).ToList();

            ProductListView.ItemsSource = products;
            TBCount.Text = "Найдено: " + products.Count;
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e) { UpdateProduct(); }
        private void ComboCategory_SelectionChanged(object sender, SelectionChangedEventArgs e) { UpdateProduct(); }
        private void RButtonUp_Checked(object sender, RoutedEventArgs e) { if (IsLoaded) UpdateProduct(); }
        private void RButtonDown_Checked(object sender, RoutedEventArgs e) { if (IsLoaded) UpdateProduct(); }
        private void Button_Click(object sender, RoutedEventArgs e) { ManagerClass.MainFrame.Navigate(new AddEditPage()); }
    }
    
}
