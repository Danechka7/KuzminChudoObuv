using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml.Linq;

namespace KuzminChudoObuv
{
    public partial class AnalyticsPage : Page
    {
        public AnalyticsPage()
        {
            InitializeComponent();
            DateTime today = DateTime.Today;
            StartDatePicker.SelectedDate = new DateTime(today.Year, today.Month, 1);
            EndDatePicker.SelectedDate = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
            Recalculate();
        }
        private void DateChanged(object sender, SelectionChangedEventArgs e)
        {
            Recalculate();
        }
        private void Recalculate()
        {
            if (StartDatePicker == null || EndDatePicker == null) return;
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null) return;
            DateTime start = StartDatePicker.SelectedDate.Value.Date;
            DateTime end = EndDatePicker.SelectedDate.Value.Date;
            if (start > end)
            {
                ErrorText.Text = "Дата начала не может быть позже даты окончания.";
                TotalOrdersText.Text = "Всего заказов: 0";
                TotalSumText.Text = "Общая сумма продаж: 0,00 ₽";
                TopProductsListView.ItemsSource = null;
                return;
            }
            ErrorText.Text = "";
            var context = Kuzmin_ChudoObuvEntities.GetContext();
            var orders = context.Orders.ToList().Where(o => o.OrderDate.Date >= start && o.OrderDate.Date <= end).ToList();
            TotalOrdersText.Text = "Всего заказов: " + orders.Count;
            decimal total = 0;
            var allItems = new List<OrderProducts>();
            foreach (var order in orders)
            {
                if (order.OrderProducts == null) continue;
                foreach (var item in order.OrderProducts)
                {
                    total += item.Quantity * item.PriceAtOrderTime;
                    allItems.Add(item);
                }
            }
            TotalSumText.Text = "Общая сумма продаж: " + total.ToString("N2") + " ₽";
            var top = allItems
                .Where(i => i.StockItems != null && i.StockItems.Products != null)
                .GroupBy(i => i.StockItems.Products.ID_Product)
                .Select(g => new
                {
                    ProductName = g.First().StockItems.Products.ProductName,
                    Manufacture = g.First().StockItems.Products.Manufactures != null ? g.First().StockItems.Products.Manufactures.ManufactureName : "",
                    TotalPairs = g.Sum(i => i.Quantity)
                }).OrderByDescending(x => x.TotalPairs).Take(5).ToList();
            TopProductsListView.ItemsSource = top;
        }
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите период.");
                return;
            }
            DateTime start = StartDatePicker.SelectedDate.Value.Date;
            DateTime end = EndDatePicker.SelectedDate.Value.Date;
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "PDF файл (*.pdf)|*.pdf";
            dialog.FileName = "Отчёт_" + start.ToString("dd.MM.yyyy") + "_" + end.ToString("dd.MM.yyyy") + ".pdf";
            if (dialog.ShowDialog() != true) return;
            string path = dialog.FileName;
            var context = Kuzmin_ChudoObuvEntities.GetContext();
            var orders = context.Orders.ToList().Where(o => o.OrderDate.Date >= start && o.OrderDate.Date <= end).ToList();
            decimal total = 0;
            var allItems = new List<OrderProducts>();
            foreach (var order in orders)
            {
                if (order.OrderProducts == null) continue;
                foreach (var item in order.OrderProducts)
                {
                    total += item.Quantity * item.PriceAtOrderTime;
                    allItems.Add(item);
                }
            }
            var top = allItems
                .Where(i => i.StockItems != null && i.StockItems.Products != null)
                .GroupBy(i => i.StockItems.Products.ID_Product)
                .Select(g => new
                {
                    ProductName = g.First().StockItems.Products.ProductName,
                    Manufacture = g.First().StockItems.Products.Manufactures != null ? g.First().StockItems.Products.Manufactures.ManufactureName : "",
                    TotalPairs = g.Sum(i => i.Quantity)
                }).OrderByDescending(x => x.TotalPairs).Take(5).ToList();
            string fontPath = "C:\\Windows\\Fonts\\arial.ttf";
            iTextSharp.text.pdf.BaseFont baseFont = iTextSharp.text.pdf.BaseFont.CreateFont(fontPath, iTextSharp.text.pdf.BaseFont.IDENTITY_H, iTextSharp.text.pdf.BaseFont.EMBEDDED);
            iTextSharp.text.Font titleFont = new iTextSharp.text.Font(baseFont, 18, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Font boldFont = new iTextSharp.text.Font(baseFont, 12, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Font normalFont = new iTextSharp.text.Font(baseFont, 12);
            iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 40, 40, 40, 40);
            iTextSharp.text.pdf.PdfWriter.GetInstance(doc, new System.IO.FileStream(path, System.IO.FileMode.Create));
            doc.Open();
            doc.Add(new iTextSharp.text.Paragraph("Отчёт по продажам", titleFont));
            doc.Add(new iTextSharp.text.Paragraph("Период: " + start.ToString("dd.MM.yyyy") + " — " + end.ToString("dd.MM.yyyy"), normalFont));
            doc.Add(new iTextSharp.text.Paragraph(" "));
            doc.Add(new iTextSharp.text.Paragraph("Всего заказов: " + orders.Count, boldFont));
            doc.Add(new iTextSharp.text.Paragraph("Общая сумма продаж: " + total.ToString("N2") + " ₽", boldFont));
            doc.Add(new iTextSharp.text.Paragraph(" "));
            doc.Add(new iTextSharp.text.Paragraph("Топ-5 моделей по проданным парам", boldFont));
            doc.Add(new iTextSharp.text.Paragraph(" "));
            iTextSharp.text.pdf.PdfPTable table = new iTextSharp.text.pdf.PdfPTable(3);
            table.WidthPercentage = 100;
            table.AddCell(new iTextSharp.text.Phrase("Модель", boldFont));
            table.AddCell(new iTextSharp.text.Phrase("Производитель", boldFont));
            table.AddCell(new iTextSharp.text.Phrase("Пар продано", boldFont));
            foreach (var item in top)
            {
                table.AddCell(new iTextSharp.text.Phrase(item.ProductName, normalFont));
                table.AddCell(new iTextSharp.text.Phrase(item.Manufacture, normalFont));
                table.AddCell(new iTextSharp.text.Phrase(item.TotalPairs.ToString(), normalFont));
            }
            doc.Add(table);
            doc.Close();
            MessageBox.Show("Отчёт сохранён: " + path);
        }
    }
}