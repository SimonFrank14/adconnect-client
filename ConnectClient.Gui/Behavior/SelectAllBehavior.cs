using Microsoft.Xaml.Behaviors;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace ConnectClient.Gui.Behavior
{
    public class SelectAllBehavior : Behavior<Button>
    {
        public static DependencyProperty SelectionBehaviorProperty = DependencyProperty.Register("SelectionBehavior", typeof(SelectionBehavior), typeof(SelectAllBehavior));

        public SelectionBehavior SelectionBehavior
        {
            get { return (SelectionBehavior)GetValue(SelectionBehaviorProperty); }
            set { SetValue(SelectionBehaviorProperty, value); }
        }

        public static DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(IEnumerable), typeof(SelectAllBehavior));

        public IEnumerable Items
        {

            get { return (IEnumerable)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public static DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems", typeof(IList), typeof(SelectAllBehavior));

        public IList SelectedItems
        {

            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Click += OnButtonClick;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Click -= OnButtonClick;
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (SelectionBehavior == SelectionBehavior.Select)
            {
                SelectAll();
            }
            else if (SelectionBehavior == SelectionBehavior.Unselect)
            {
                UnselectAll();
            }
        }

        private void SelectAll()
        {
            foreach (var item in Items)
            {
                if (!SelectedItems.Contains(item))
                {
                    SelectedItems.Add(item);
                }
            }
        }

        private void UnselectAll()
        {
            foreach(var item in Items)
            {
                if(SelectedItems.Contains(item))
                {
                    SelectedItems.Remove(item);
                }
            }
        }
    }
}
