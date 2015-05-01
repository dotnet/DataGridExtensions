namespace DataGridExtensions
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Threading;

    internal static class ExtensionMethods
    {
        /// <summary>
        /// Restarts the specified timer.
        /// </summary>
        /// <param name="timer">The timer.</param>
        internal static void Restart(this DispatcherTimer timer)
        {
            Contract.Requires(timer != null);

            timer.Stop();
            timer.Start();
        }

        /// <summary>
        /// Walks the elements tree and returns the first element that derives from T.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="item">The item to start search with.</param>
        /// <returns>The element if found; otherwise null.</returns>
        internal static T FindAncestorOrSelf<T>(this DependencyObject item) where T : class
        {
            while (item != null)
            {
                var target = item as T;
                if (target != null)
                    return target;

                item = LogicalTreeHelper.GetParent(item) ?? VisualTreeHelper.GetParent(item);
            }

            return null;
        }

        /// <summary>
        /// Shortcut to <see cref="System.Windows.Threading.Dispatcher.BeginInvoke(Delegate, Object[])"/>
        /// </summary>
        public static void BeginInvoke(this DispatcherObject self, Action action)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (action == null)
                throw new ArgumentNullException("action");

            self.Dispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// Shortcut to <see cref="System.Windows.Threading.Dispatcher.BeginInvoke(DispatcherPriority, Delegate)"/>
        /// </summary>
        public static void BeginInvoke(this DispatcherObject self, DispatcherPriority priority, Action action)
        {
            if (self == null)
                throw new ArgumentNullException("self");
            if (action == null)
                throw new ArgumentNullException("action");

            self.Dispatcher.BeginInvoke(priority, action);
        }

        /// <summary>
        /// Performs a cast from object to <typeparamref name="T"/>, avoiding possible null violations if <typeparamref name="T"/> is a value type.
        /// </summary>
        /// <typeparam name="T">The target type</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The value casted to <typeparamref name="T"/>, or <c>default(T)</c> if value is <c>null</c>.</returns>
        public static T SafeCast<T>(this object value)
        {
            return (value == null) ? default(T) : (T)value;
        }

        /// <summary>
        /// Gets the value of a dependency property using <see cref="SafeCast{T}(object)" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self">The dependency object from which to get the value.</param>
        /// <param name="property">The property to get.</param>
        /// <returns>The value safely casted to <typeparamref name="T"/></returns>
        public static T GetValue<T>(this DependencyObject self, DependencyProperty property)
        {
            Contract.Requires(self != null);
            Contract.Requires(property != null);

            return self.GetValue(property).SafeCast<T>();
        }

    }
}
