namespace DataGridExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Threading;

    internal static class InternalExtensionMethods
    {
        /// <summary>
        /// Restarts the specified timer.
        /// </summary>
        /// <param name="timer">The timer.</param>
        internal static void Restart(this DispatcherTimer timer)
        {
            timer.Stop();
            timer.Start();
        }

        /// <summary>
        /// Walks the elements tree and returns the first element that derives from T.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="item">The item to start search with.</param>
        /// <returns>The element if found; otherwise null.</returns>
        internal static T? FindAncestorOrSelf<T>(this DependencyObject? item) where T : class
        {
            while (item != null)
            {
                if (item is T target)
                    return target;

                item = LogicalTreeHelper.GetParent(item) ?? VisualTreeHelper.GetParent(item);
            }

            return null;
        }

        public static IEnumerable<DependencyObject> AncestorsAndSelf(this DependencyObject self)
        {
            while (self != null)
            {
                yield return self;
                self = LogicalTreeHelper.GetParent(self) ?? VisualTreeHelper.GetParent(self);
            }
        }

        /// <summary>
        /// Shortcut to <see cref="System.Windows.Threading.Dispatcher.BeginInvoke(Delegate, Object[])"/>
        /// </summary>
        public static void BeginInvoke(this Visual self, Action action)
        {
            self.Dispatcher?.BeginInvoke(action);
        }

        /// <summary>
        /// Shortcut to <see cref="System.Windows.Threading.Dispatcher.BeginInvoke(DispatcherPriority, Delegate)"/>
        /// </summary>
        public static void BeginInvoke(this Visual self, DispatcherPriority priority, Action action)
        {
            self.Dispatcher?.BeginInvoke(priority, action);
        }
    }
}
