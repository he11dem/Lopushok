using Lopushok.DB;
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
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public static List<TypeProduct> typees { get; set; }
        public static List<Product> products { get; set; }

        private int _pageSize = 20;
        private int _pageNumber = 1;
        private int _pageCount = 0;
        public ProductPage()
        {
            InitializeComponent();
            products = new List<Product>(DBConnection.lopush.Product.ToList());
            typees = new List<TypeProduct>(DBConnection.lopush.TypeProduct.ToList());
            Refresh();
            var allTypesProduct = new TypeProduct
            {
                IdTypeProduct = 0,
                NameTypeProduct = "Все типы"
            };
            typees.Add(allTypesProduct);

            ProductLV.ItemsSource = new List<Product>(DBConnection.lopush.Product.ToList());
            this.DataContext = this;
        }
        private void SearchTbx_TextChanged(object sender, TextChangedEventArgs e)
        {
            Refresh();
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddProductPage());
        }



        private void DeleteBtn_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Product product)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить этот товар?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        DBConnection.lopush.Product.Remove(product);
                        DBConnection.lopush.SaveChanges();
                        NavigationService.Navigate(new ProductPage());
                        MessageBox.Show("Товар успешно удалён!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Товар невозможно удалить, так как он находится в рассписании", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }
        }

        private void SortCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (SortCb.SelectedItem != null)
            {
                SortCb.Text = ((ComboBoxItem)SortCb.SelectedItem).Content.ToString();

            }
            else
            {
                SortCb.Text = "Сортировка"; //  Если выделение снято, возвращаем текст по умолчанию.
            }
            Refresh();
        }

        private void Refresh()
        {
            var filterProduct = DBConnection.lopush.Product.ToList();
            var category = FilterCb.SelectedItem as TypeProduct;

            if (SearchTbx.Text.Length > 0)
            {
                filterProduct = filterProduct.Where(i => i.NameProduct.ToLower().StartsWith(SearchTbx.Text.Trim().ToLower())).ToList();
            }

            if (category != null && category.IdTypeProduct != 0)
            {
                filterProduct = filterProduct.Where(d => d.IdTypeProduct == category.IdTypeProduct).ToList();
            }

            switch (SortCb.SelectedIndex)
            {
                case 0: // "Нет" - не сортируем
                    break;
                case 1: // "По возрастанию"
                    filterProduct = filterProduct.OrderBy(x =>
                    {
                        if (x.Product_Materials == null || !x.Product_Materials.Any())
                        {
                            return 0;
                        }

                        decimal total = (decimal)x.Product_Materials.Sum(pm =>
                        {
                            decimal cost = pm.Materials?.Cost ?? 0;
                            return cost * pm.NumberMaterials;
                        });

                        return total;
                    }).ToList();
                    break;
                case 2: // "По убыванию"
                    filterProduct = filterProduct.OrderByDescending(x =>
                    {
                        if (x.Product_Materials == null || !x.Product_Materials.Any())
                        {
                            return 0;
                        }

                        decimal total = (decimal)x.Product_Materials.Sum(pm =>
                        {
                            decimal cost = pm.Materials?.Cost ?? 0;
                            return cost * pm.NumberMaterials;
                        });

                        return total;
                    }).ToList();
                    break;

                default:
                    break;
            }

            _pageCount = (int)Math.Ceiling((double)filterProduct.Count / _pageSize); // Вычисляем количество страниц
            filterProduct = filterProduct.Skip((_pageNumber - 1) * _pageSize).Take(_pageSize).ToList();

            UpdateNavigation();

            ProductLV.ItemsSource = filterProduct;





        }

        private void UpdateNavigation()
        {
            NavSp.Children.Clear();

            if (_pageCount > 1)
            {
                // Кнопка "Предыдущая"
                Button button1 = new Button
                {
                    Content = "<",
                    IsHitTestVisible = _pageNumber > 1,
                    Background = new SolidColorBrush(Colors.Transparent),
                    BorderBrush = new SolidColorBrush(Colors.Transparent),
                };
                button1.Click += PageBtn_Click;
                NavSp.Children.Add(button1);

                // Кнопки для каждой страницы
                for (int i = 1; i <= _pageCount; i++)
                {
                    TextBlock textBlock = new TextBlock()
                    {
                        Text = i.ToString(),
                        TextDecorations = i == _pageNumber ? TextDecorations.Underline : null // Подчеркиваем текущую страницу
                    };

                    Button button2 = new Button
                    {
                        Content = textBlock,
                        IsHitTestVisible = i != _pageNumber,
                        Background = new SolidColorBrush(Colors.Transparent),
                        BorderBrush = new SolidColorBrush(Colors.Transparent)
                    };
                    button2.Click += PageBtn_Click;
                    button2.Tag = i; // Сохраняем номер страницы в Tag
                    NavSp.Children.Add(button2);
                }

                // Кнопка "Следующая"
                Button button3 = new Button
                {
                    Content = ">",
                    IsHitTestVisible = _pageNumber < _pageCount,
                    Background = new SolidColorBrush(Colors.Transparent),
                    BorderBrush = new SolidColorBrush(Colors.Transparent)
                };
                button3.Click += PageBtn_Click;
                NavSp.Children.Add(button3);
            }
        }

        private void PageBtn_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            switch (button.Content.ToString())
            {
                case "<":
                    _pageNumber--;
                    break;
                case ">":
                    _pageNumber++;
                    break;
                default:
                    _pageNumber = int.Parse(((TextBlock)button.Content).Text);
                    break;
            }
            Refresh();
        }

        private void FilterCbClik(object sender, SelectionChangedEventArgs e)
        {
            if (FilterCb.SelectedItem != null)
            {
                if (FilterCb.SelectedIndex == 0)
                {
                    FilterCb.Text = "Фильтрация";
                }
                else
                {
                    FilterCb.Text = (FilterCb.SelectedItem as TypeProduct)?.NameTypeProduct ?? "Фильтрация";
                }
            }
            else
            {
                FilterCb.Text = "Фильтрация";
            }
            Refresh();
        }

        private void ProductLV_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (ProductLV.SelectedItem is Product product)
            {
                product = ProductLV.SelectedItem as Product;
                NavigationService.Navigate(new EditProductPage(product));
            }
        }
    }
}
