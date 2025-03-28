using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace RimworldModTranslator.Behaviors
{
    public static class DataGridBehavior
    {
        // Define the attached property
        public static readonly DependencyProperty AutoScrollToSelectedItemProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToSelectedItem",
                typeof(bool),
                typeof(DataGridBehavior),
                new PropertyMetadata(false, OnAutoScrollToSelectedItemChanged));

        // Getter for the property
        public static bool GetAutoScrollToSelectedItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollToSelectedItemProperty);
        }

        // Setter for the property
        public static void SetAutoScrollToSelectedItem(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollToSelectedItemProperty, value);
        }

        // Callback when the property value changes
        private static void OnAutoScrollToSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if ((bool)e.NewValue)
                {
                    // Subscribe to the SelectionChanged event when property is set to true
                    dataGrid.SelectionChanged += DataGrid_SelectionChanged;
                }
                else
                {
                    // Unsubscribe when property is set to false
                    dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
                }
            }
        }

        // Event handler for SelectionChanged
        private static void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (dataGrid?.SelectedItem != null)
            {
                // Scroll the selected item into view
                dataGrid.ScrollIntoView(dataGrid.SelectedItem);
            }
        }
    }
}
