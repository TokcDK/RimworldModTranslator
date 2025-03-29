using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RimworldModTranslator.Extensions
{

    public static class DataGridExtensions
    {
        public static readonly DependencyProperty ColumnsSourceProperty =
            DependencyProperty.RegisterAttached(
                "ColumnsSource",
                typeof(ObservableCollection<string>),
                typeof(DataGridExtensions),
                new PropertyMetadata(null, OnColumnsSourceChanged));

        public static ObservableCollection<string> GetColumnsSource(DependencyObject obj)
        {
            return (ObservableCollection<string>)obj.GetValue(ColumnsSourceProperty);
        }

        public static void SetColumnsSource(DependencyObject obj, ObservableCollection<string> value)
        {
            obj.SetValue(ColumnsSourceProperty, value);
        }

        private static void OnColumnsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                dataGrid.Columns.Clear();
                if (e.NewValue is ObservableCollection<string> columnNames)
                {
                    foreach (var columnName in columnNames)
                    {
                        dataGrid.Columns.Add(new DataGridTextColumn
                        {
                            Header = columnName,
                            Binding = new Binding($"[{columnName}]")
                        });
                    }
                }
            }
        }
    }
}
