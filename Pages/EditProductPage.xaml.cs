using Lopushok.DB;
using Lopushok.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Lopushok.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditProductPage.xaml
    /// </summary>
    public partial class EditProductPage : Page
    {
        public static List<TypeProduct> types = new List<TypeProduct>();
        public static List<Materials> materials = new List<Materials>();
        public static List<Product_Materials> product_s = new List<Product_Materials>();
        public static Product product1 = new Product();
        public Materials SelectedMaterial { get; private set; }


        Product contextProduct;
        private Product product;

        public static Product pro { get; set; }
        public EditProductPage(Product product)
        {
            InitializeComponent();
            product1 = product;
            contextProduct = product;
            pro = product;
            this.DataContext = this;
            LoadProductData();
        }


        private void LoadProductData()
        {
            NameTb.Text = contextProduct.NameProduct;
            ArticleTb.Text = Convert.ToString(contextProduct.ArticleNumber);
            TotalCostTbx.Text = contextProduct.TotalCost;
            MaterialsGrid.ItemsSource = product1.Product_Materials.ToList();
        }
        private void UpdateTotalCost()
        {
            if (product1.Product_Materials == null || !product1.Product_Materials.Any())
            {
                // Если материалов нет, стоимость равна 0
                TotalCostTbx.Text = "0.00 руб.";
                return;
            }

            // Считаем общую стоимость
            decimal totalCost = product1.Product_Materials
    .Sum(pm => (pm.Materials?.Cost ?? 0) * (pm.NumberMaterials ?? 0));

            // Обновляем текстовое поле
            TotalCostTbx.Text = $"{totalCost:N2} руб.";



        }



        private void ChangeImageBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "*.png|*.png|*.jpeg|*.jpeg|*.jpg|*.jpg"
            };
            if (openFileDialog.ShowDialog().GetValueOrDefault())
            {
                product1.PhotoBinary = File.ReadAllBytes(openFileDialog.FileName);
                TestImg.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            };
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProductPage());
        }



        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var material = (sender as Button)?.DataContext as Product_Materials;
            if (material != null)
            {
                // Показываем диалоговое окно с подтверждением
                MessageBoxResult result = MessageBox.Show(
                    "Вы точно хотите удалить этот материал?", // Текст сообщения
                    "Подтверждение удаления",                // Заголовок окна
                    MessageBoxButton.YesNo,                 // Кнопки "Да" и "Нет"
                    MessageBoxImage.Question               // Иконка вопроса
                );

                // Если пользователь подтвердил удаление
                if (result == MessageBoxResult.Yes)
                {
                    // Удаляем материал из списка
                    product1.Product_Materials.Remove(material);

                    // Обновляем DataGrid
                    MaterialsGrid.ItemsSource = product1.Product_Materials.ToList();

                    // Обновляем общую стоимость
                    UpdateTotalCost();

                    // Показываем сообщение об успешном удалении
                    MessageBox.Show(
                        "Материал успешно удалён!",
                        "Информация",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }
                else
                {
                    // Пользователь отменил удаление
                    MessageBox.Show(
                        "Удаление отменено.",
                        "Информация",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                }

            }

        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Product product = product1;
            if (NameTb.Text == "" || ArticleTb.Text == "" || TotalCostTbx.Text == "" || TestImg.Source == null)
            {
                MessageBox.Show("Заполните все данные!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {

                product1.NameProduct = NameTb.Text;
                product1.ArticleNumber = Convert.ToInt32(ArticleTb.Text);
                UpdateTotalCost();
                DBConnection.lopush.SaveChanges();
                MessageBox.Show("Данные изменены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new ProductPage());
            }
        }

        private void CalCostBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateTotalCost();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var addMaterialWindow = new SelectedMaterial();
            if (addMaterialWindow.ShowDialog() == true)
            {
                var selectedMaterial = addMaterialWindow.SelectedMateriall;
                var quantity = addMaterialWindow.Quantity;

                // Создаём новую запись в таблице Product_Materials
                var productMaterial = new Product_Materials
                {
                    IdMaterials = selectedMaterial.IdMaterials,
                    IdProduct = product1.IdProduct,
                    NumberMaterials = quantity
                };

                // Добавляем запись в таблицу Product_Materials
                product1.Product_Materials.Add(productMaterial);

                // Сохраняем изменения в базе данных
                try
                {
                    DBConnection.lopush.SaveChanges();
                    MessageBox.Show("Материал успешно добавлен к продукту.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    UpdateTotalCost();
                    MaterialsGrid.ItemsSource = product1.Product_Materials.ToList();

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
