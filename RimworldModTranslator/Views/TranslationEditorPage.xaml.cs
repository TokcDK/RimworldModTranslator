using RimworldModTranslator.Helpers;
using RimworldModTranslator.Models.EditorColumns;
using RimworldModTranslator.ViewModels;
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

namespace RimworldModTranslator.Views
{
    /// <summary>
    /// Логика взаимодействия для TranslationEditorPage.xaml
    /// </summary>
    public partial class TranslationEditorPage : UserControl
    {
        public TranslationEditorPage()
        {
            InitializeComponent();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (EditorHelper.IsReadOnlyColumn(e.Column.Header.ToString()))
            {
                e.Column.IsReadOnly = true;
            }
            e.Column.Width = 100;
        }
    }
}
