using RimworldModTranslator.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace RimworldModTranslator.Views
{
    /// <summary>
    /// Логика взаимодействия для SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        public SearchWindow()
        {
            InitializeComponent();
        }
        private void FoundItemsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is SearchWindowViewModel viewModel && sender is DataGrid dataGrid)
            {
                if (dataGrid.SelectedItem is DataRowView selectedRow)
                {
                    viewModel.OnFoundItemSelected(selectedRow);
                }
            }
        }
    }
}
