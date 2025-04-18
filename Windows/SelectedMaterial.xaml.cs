using Lopushok.DB;
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
using System.Windows.Shapes;

namespace Lopushok.Windows
{
    /// <summary>
    /// Логика взаимодействия для SelectedMaterial.xaml
    /// </summary>
    public partial class SelectedMaterial : Window
    {
        public static Materials materials1 = new Materials();
        public Materials SelectedMateriall { get; private set; }
        public int Quantity { get; private set; }
        public SelectedMaterial()
        {
            InitializeComponent();
            LoadMaterials();
        }
        private void LoadMaterials()
        {
            var materials1 = DBConnection.lopush.Materials.ToList();
            MaterialComboBox.ItemsSource = new List<Materials>(DBConnection.lopush.Materials.ToList());
            DataContext = this;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите материал.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SelectedMateriall = (Materials)MaterialComboBox.SelectedItem;
            Quantity = quantity;

            DialogResult = true;

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
