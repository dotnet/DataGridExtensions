namespace DataGridExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
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

        public static IEnumerable<DependencyObject> AncestorsAndSelf(this DependencyObject self)
        {
            Contract.Requires(self != null);
            Contract.Ensures(Contract.Result<IEnumerable<DependencyObject>>() != null);

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
            Contract.Requires(self != null);
            Contract.Requires(action != null);

            var dispatcher = self.Dispatcher;
            Contract.Assume(dispatcher != null);
            dispatcher.BeginInvoke(action);
        }

        /// <summary>
        /// Shortcut to <see cref="System.Windows.Threading.Dispatcher.BeginInvoke(DispatcherPriority, Delegate)"/>
        /// </summary>
        public static void BeginInvoke(this Visual self, DispatcherPriority priority, Action action)
        {
            Contract.Requires(self != null);
            Contract.Requires(action != null);

            var dispatcher = self.Dispatcher;
            Contract.Assume(dispatcher != null);
            dispatcher.BeginInvoke(priority, action);
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

        /// <summary>
        /// Performs the specified action on each element of the collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="action">The action.</param>
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            Contract.Requires(collection != null);
            Contract.Requires(action != null);

            foreach (var item in collection)
            {
                action(item);
            }
        }
    }

    internal static class Enumerate
    {
        /// <summary>
        /// Enumerates the elements of two enumerations as tuples.
        /// </summary>
        /// <typeparam name="T1">The type of the first collection.</typeparam>
        /// <typeparam name="T2">The type of the second collection.</typeparam>
        /// <param name="first">The first collection.</param>
        /// <param name="second">The second collection.</param>
        /// <returns>Tuples of the elements.</returns>
        /// <remarks>If the number of elements in each collection is different, the smaller collection determines the number of enumerated items.</remarks>
        public static IEnumerable<Tuple<T1, T2>> AsTuples<T1, T2>(IEnumerable<T1> first, IEnumerable<T2> second)
        {
            Contract.Requires(first != null);
            Contract.Requires(second != null);
            Contract.Ensures(Contract.Result<IEnumerable<Tuple<T1, T2>>>() != null);

            using (var e1 = first.GetEnumerator())
            {
                using (var e2 = second.GetEnumerator())
                {
                    while (e1.MoveNext() && e2.MoveNext())
                    {
                        yield return new Tuple<T1, T2>(e1.Current, e2.Current);
                    }
                }
            }
        }
    }
}
