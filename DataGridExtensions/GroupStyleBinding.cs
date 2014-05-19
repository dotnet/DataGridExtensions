namespace DataGridExtensions
{
    using System.Collections;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Helper class to support binding to the group style from within a style setter.
    /// </summary>
    public static class GroupStyleBinding
    {
        /// <summary>
        /// Gets the group style.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The group style.</returns>
        public static GroupStyle GetGroupStyle(DependencyObject obj)
        {
            return (GroupStyle)obj.GetValue(GroupStyleProperty);
        }
        /// <summary>
        /// Sets the group style.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetGroupStyle(DependencyObject obj, GroupStyle value)
        {
            obj.SetValue(GroupStyleProperty, value);
        }
        /// <summary>
        /// Identifies the GroupStyle attached property.
        /// </summary>
        public static readonly DependencyProperty GroupStyleProperty =
            DependencyProperty.RegisterAttached("GroupStyle", typeof(GroupStyle), typeof(GroupStyleBinding), new FrameworkPropertyMetadata(GroupStyle_Changed));

        private static void GroupStyle_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = d as ItemsControl;
            if (itemsControl == null)
                return;

            itemsControl.GroupStyle.Clear();

            var groupStyle = e.NewValue as GroupStyle;
            if (groupStyle != null)
            {
                itemsControl.GroupStyle.Add(groupStyle);
                return;
            }

            var groupStyles = e.NewValue as IEnumerable;
            if (groupStyles != null)
            {
                foreach (var item in groupStyles.OfType<GroupStyle>())
                {
                    itemsControl.GroupStyle.Add(item);
                }
            }
        }
    }
}
