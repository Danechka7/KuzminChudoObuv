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
        private Products _product;
        private int _qty = 1;

        public ProductDetailPage(Products product)
        {
            InitializeComponent();
            _product = product;
            DataContext = _product;

            var ctx = Kuzmin_ChudoObuvEntities.GetContext();
            var sizes = ctx.StockItems
                .Where(s => s.ID_Product == _product.ID_Product && s.ItemsQuantity > 0)
                .ToList()
                .Select(s => new SizeItem
                {
                    ID_StockItem = s.ID_Item,
                    SizeText = s.Sizes != null ? s.Sizes.Size.ToString(System.Globalization.CultureInfo.InvariantCulture) : ""
                }).ToList();

            ComboSizes.ItemsSource = sizes;
            if (sizes.Count > 0) ComboSizes.SelectedIndex = 0;
        }

        private void Plus_Click(object sender, RoutedEventArgs e) { _qty++; TxtQty.Text = _qty.ToString(); }
        private void Minus_Click(object sender, RoutedEventArgs e) { if (_qty > 1) { _qty--; TxtQty.Text = _qty.ToString(); } }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var selectedSize = ComboSizes.SelectedItem as SizeItem;
            if (selectedSize == null) { MessageBox.Show("Выберите размер!"); return; }

            var ctx = Kuzmin_ChudoObuvEntities.GetContext();
            var stock = ctx.StockItems.FirstOrDefault(s => s.ID_Item == selectedSize.ID_StockItem);
            if (stock == null) return;

            var existingInCart = ManagerClass.Cart.FirstOrDefault(c => c.StockItemID == selectedSize.ID_StockItem);
            int alreadyInCart = existingInCart != null ? existingInCart.Quantity : 0;

            if (alreadyInCart + _qty > stock.ItemsQuantity)
            {
                MessageBox.Show($"На складе осталось {stock.ItemsQuantity} шт., а в корзине уже {alreadyInCart} шт.");
                return;
            }

            ManagerClass.AddToCart(selectedSize.ID_StockItem, _product.ProductName, selectedSize.SizeText, _qty, _product.ProductCost);
            MessageBox.Show("Добавлено в корзину!");
            ManagerClass.MainFrame.GoBack();
        }
    }
}