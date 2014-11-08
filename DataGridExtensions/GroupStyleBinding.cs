namespace DataGridExtensions
{
    using System.Collections;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using DataGridExtensions.Framework;

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
            Contract.Requires(obj != null);

            return (GroupStyle)obj.GetValue(GroupStyleProperty);
        }
        /// <summary>
        /// Sets the group style.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetGroupStyle(DependencyObject obj, GroupStyle value)
        {
            Contract.Requires(obj != null);

            obj.SetValue(GroupStyleProperty, value);
        }
        /// <summary>
        /// Identifies the GroupStyle attached property.
        /// </summary>
        public static readonly DependencyProperty GroupStyleProperty =
            DependencyProperty.RegisterAttached("GroupStyle", typeof(GroupStyle), typeof(GroupStyleBinding), new FrameworkPropertyMetadata(GroupStyle_Changed));

        private static void GroupStyle_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var groupStyle = d.Maybe()
                .Select(i => i as ItemsControl)
                .Return(i => i.GroupStyle);

            if (groupStyle == null)
                return;

            groupStyle.Clear();

            var newGroupStyle = e.NewValue as GroupStyle;
            if (newGroupStyle != null)
            {
                groupStyle.Add(newGroupStyle);
                return;
            }

            var groupStyles = e.NewValue as IEnumerable;
            if (groupStyles != null)
            {
                foreach (var item in groupStyles.OfType<GroupStyle>())
                {
                    groupStyle.Add(item);
                }
            }
        }
    }
}
