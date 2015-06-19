namespace DataGridExtensions.Framework
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading;
    using System.Windows.Threading;

    /// <summary>
    /// Implements a throttle that uses the dispatcher to delay the target action.
    /// </summary>
    public class DispatcherThrottle
    {
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
        private readonly Action _target;
        private readonly DispatcherPriority _priority;

        private int _counter;

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherThrottle"/> class.
        /// </summary>
        /// <param name="target">The target action to invoke when the throttle condition is hit.</param>
        public DispatcherThrottle(Action target)
            : this(DispatcherPriority.Normal, target)
        {
            Contract.Requires(target != null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherThrottle" /> class.
        /// </summary>
        /// <param name="priority">The priority of the dispatcher.</param>
        /// <param name="target">The target action to invoke when the throttle condition is hit.</param>
        public DispatcherThrottle(DispatcherPriority priority, Action target)
        {
            Contract.Requires(target != null);

            _target = target;
            _priority = priority;
        }

        /// <summary>
        /// Ticks this instance to trigger the throttle.
        /// </summary>
        public void Tick()
        {
            Interlocked.Increment(ref _counter);

            _dispatcher.BeginInvoke(_priority, (Action)delegate
            {
                if (Interlocked.Decrement(ref _counter) != 0)
                    return;

                _target();
            });
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(_dispatcher != null);
            Contract.Invariant(_target != null);
        }
    }
}
