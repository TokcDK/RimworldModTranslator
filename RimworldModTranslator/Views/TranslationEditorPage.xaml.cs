using RimworldModTranslator.Helpers;
using System.Windows.Controls;

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

                EditorHelper.SetColumnHeaderToCaption(e, EditorTable);
                e.Column.Width = 70;
            }
            else
            {
                e.Column.Width = 100;
            }
        }
    }
}
