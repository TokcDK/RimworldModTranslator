using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace RimworldModTranslator.Behaviors
{
    public static class ListBoxAutoScrollToBottomBehavior
    {
        public static readonly DependencyProperty AutoScrollToBottomProperty =
            DependencyProperty.RegisterAttached("AutoScrollToBottom", typeof(bool), typeof(ListBoxAutoScrollToBottomBehavior),
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
                if (autoScroll)
                {
                    // Subscribe to ItemsSource changes
                    DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ListBox))
                        .AddValueChanged(listBox, (s, args) => SubscribeToCollection(listBox));

                    // Try subscribing immediately in case ItemsSource is already set
                    SubscribeToCollection(listBox);
                }
                else
                {
                    // Optionally clean up (unsubscribe) if auto-scroll is disabled
                    DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ListBox))
                        .RemoveValueChanged(listBox, (s, args) => SubscribeToCollection(listBox));
                }
            }
        }

        private static void SubscribeToCollection(ListBox listBox)
        {
            // Unsubscribe from previous collection if any (to prevent leaks)
            if (listBox.GetValue(PreviousCollectionProperty) is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= (s, args) => OnCollectionChanged(listBox, args);
            }

            // Subscribe to the new collection
            if (listBox.ItemsSource is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged += (s, args) => OnCollectionChanged(listBox, args);
                listBox.SetValue(PreviousCollectionProperty, collection); // Store reference
            }
            else
            {
                listBox.SetValue(PreviousCollectionProperty, null);
            }
        }

        private static void OnCollectionChanged(ListBox listBox, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add && listBox.Items.Count > 0)
            {
                listBox.ScrollIntoView(listBox.Items[listBox.Items.Count - 1]);
            }
        }

        // Store previous collection to allow cleanup
        private static readonly DependencyProperty PreviousCollectionProperty =
            DependencyProperty.RegisterAttached("PreviousCollection", typeof(INotifyCollectionChanged), typeof(ListBoxAutoScrollToBottomBehavior));
    }
}