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
    /// Логика взаимодействия для AddProductPage.xaml
    /// </summary>
    public partial class AddProductPage : Page
    {
        public static List<TypeProduct> types = new List<TypeProduct>();
        public static List<Materials> materials = new List<Materials>();
        public static List<Product_Materials> product_s = new List<Product_Materials>();
        public static Product product1 = new Product();
        public Materials SelectedMaterial { get; private set; }

        public static Product pro { get; set; }

        public AddProductPage()
        {
            InitializeComponent();
            LoadTypeProduct();
            LoadProductData();
        }
        private void LoadProductData()
        {
            // Очищаем поля для нового продукта
            NameTb.Text = string.Empty;
            ArticleTb.Text = string.Empty;
            TestImg.Source = null;
        }

        private void LoadTypeProduct()
        {
            TypeCb.ItemsSource = new List<TypeProduct>(DBConnection.lopush.TypeProduct.ToList());
            DataContext = this;
        }


        private void AddImageBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "*.png|*.png|*.jpeg|*.jpeg|*.jpg|*.jpg"
            };
            if (openFileDialog.ShowDialog().GetValueOrDefault())
            {
                product1.PhotoBinary = File.ReadAllBytes(openFileDialog.FileName);
                TestImg.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }



        private void AddProductBtn_Click(object sender, RoutedEventArgs e)
        {
            if (NameTb.Text == "" || ArticleTb.Text == "" || TypeCb.Text == "" || TestImg.Source == null)
            {
                MessageBox.Show("Заполните все данные!!!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                var selectedProductType = (TypeProduct)TypeCb.SelectedItem;
                // Создаем новый продукт
                Product newProduct = new Product
                {
                    NameProduct = NameTb.Text,
                    ArticleNumber = Convert.ToInt32(ArticleTb.Text),
                    PhotoBinary = product1.PhotoBinary,
                    TypeProduct = selectedProductType,
                    Product_Materials = product1.Product_Materials,

                };

                // Добавляем продукт в базу данных
                DBConnection.lopush.Product.Add(newProduct);
                DBConnection.lopush.SaveChanges();

                MessageBox.Show("Продукт успешно добавлен!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new ProductPage());
            }
        }

        private void CancelBtn_Click_1(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProductPage());
        }

        private void AddBtn_Click_1(object sender, RoutedEventArgs e)
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
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddProductPage());
        }

        private void Delete_Click_1(object sender, RoutedEventArgs e)
        {
            var material = (sender as Button)?.DataContext as Product_Materials;
            if (material != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Вы точно хотите удалить этот материал?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    product1.Product_Materials.Remove(material);
                    MessageBox.Show("Материал успешно удалён!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                else
                {
                    MessageBox.Show("Удаление отменено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
