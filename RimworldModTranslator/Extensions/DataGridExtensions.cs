using RimworldModTranslator.Helpers;
using System.Collections.Generic;
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
                typeof(IEnumerable<string>),
                typeof(DataGridExtensions),
                new PropertyMetadata(null, OnColumnsSourceChanged));

        public static IEnumerable<string> GetColumnsSource(DependencyObject obj)
        {
            return (IEnumerable<string>)obj.GetValue(ColumnsSourceProperty);
        }

        public static void SetColumnsSource(DependencyObject obj, IEnumerable<string> value)
        {
            obj.SetValue(ColumnsSourceProperty, value);
        }

        private static void OnColumnsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                dataGrid.Columns.Clear();
                if (e.NewValue is IEnumerable<string> columnNames)
                {
                    foreach (var columnName in columnNames)
                    {
                        var column = new DataGridTextColumn
                        {
                            Header = columnName,
                            Binding = new Binding($"[{columnName}]"),
                            IsReadOnly = EditorHelper.IsReadOnlyColumn(columnName) // Set read-only for specific columns
                        };
                        dataGrid.Columns.Add(column);
                    }
                }
            }
        }
    }
}