namespace DataGridExtensions.Framework
{
    using System;
    using System.Threading;
    using System.Windows.Threading;

    /// <summary>
    /// Implements a throttle that uses the dispatcher to delay the target action.
    /// </summary>
    public class DispatcherThrottle : DispatcherObject
    {
        private int _counter;
        private readonly Action _target;
        private readonly DispatcherPriority _priority;

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherThrottle"/> class.
        /// </summary>
        /// <param name="target">The target action to invoke when the throttle condition is hit.</param>
        public DispatcherThrottle(Action target)
            : this(DispatcherPriority.Normal, target)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherThrottle" /> class.
        /// </summary>
        /// <param name="priority">The priority of the dispatcher.</param>
        /// <param name="target">The target action to invoke when the throttle condition is hit.</param>
        public DispatcherThrottle(DispatcherPriority priority, Action target)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            _target = target;
            _priority = priority;
        }

        /// <summary>
        /// Ticks this instance to trigger the throttle.
        /// </summary>
        public void Tick()
        {
            Interlocked.Increment(ref _counter);

            this.BeginInvoke(_priority, delegate
            {
                if (Interlocked.Decrement(ref _counter) != 0)
                    return;

                _target();
            });

        }
    }
}
