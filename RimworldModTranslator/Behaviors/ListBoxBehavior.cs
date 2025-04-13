using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace RimworldModTranslator.Behaviors
{
    public static class ListBoxBehavior
    {
        public static readonly DependencyProperty AutoScrollToBottomProperty =
            DependencyProperty.RegisterAttached("AutoScrollToBottom", typeof(bool), typeof(ListBoxBehavior),
                new PropertyMetadata(false, OnAutoScrollToBottomChanged));

        public static bool GetAutoScrollToBottom(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollToBottomProperty);
        }

        public static void SetAutoScrollToBottom(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollToBottomProperty, value);
        }

        private static void OnAutoScrollToBottomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListBox listBox)
            {
                bool autoScroll = (bool)e.NewValue;
                if (autoScroll && listBox.ItemsSource is INotifyCollectionChanged collection)
                {
                    collection.CollectionChanged += (s, args) =>
                    {
                        if (args.Action == NotifyCollectionChangedAction.Add && listBox.Items.Count > 0)
                        {
                            listBox.ScrollIntoView(listBox.Items[listBox.Items.Count - 1]);
                        }
                    };
                }
            }
        }
    }
}
