using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace RimworldModTranslator.Behaviors
{
    public class DataGridSelectedCellsBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty SelectedCellsProperty =
            DependencyProperty.Register(nameof(SelectedCells), typeof(IList<DataGridCellInfo>),
                typeof(DataGridSelectedCellsBehavior), new PropertyMetadata(null));

        public IList<DataGridCellInfo> SelectedCells
        {
            get => (IList<DataGridCellInfo>)GetValue(SelectedCellsProperty);
            set => SetValue(SelectedCellsProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectedCellsChanged += DataGrid_SelectedCellsChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectedCellsChanged -= DataGrid_SelectedCellsChanged;
            base.OnDetaching();
        }

        private void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            SelectedCells = AssociatedObject.SelectedCells;
        }
    }
}
