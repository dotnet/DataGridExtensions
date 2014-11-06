namespace DataGridExtensions.Framework
{
    using System;

    /// <summary>
    /// Implementation of the maybe monad.
    /// </summary>
    public static class MaybeExtensions
    {
        public static Maybe<T> Maybe<T>(this T source)
            where T: class
        {
            return new Maybe<T>(source);
        }
    }

    /// <summary>
    /// Implementation of the maybe monad.
    /// </summary>
    public class Maybe<T>
        where T : class
    {
        private readonly T _source;

        /// <summary>
        /// Initializes a new instance of the <see cref="Maybe{T}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public Maybe(T source)
        {
            _source = source;
        }

        public Maybe<TTarget> Select<TTarget>(Func<T, TTarget> selector)
            where TTarget : class
        {
            return new Maybe<TTarget>((_source == null) ? null : selector(_source));
        }

        public TTarget Return<TTarget>(Func<T, TTarget> selector)
        {
            return Return(selector, default(TTarget));
        }

        public TTarget Return<TTarget>(Func<T, TTarget> selector, TTarget fallbackValue)
        {
            return (_source == null) ? fallbackValue : selector(_source);
        }

        public Maybe<T> Do(Action<T> action)
        {
            if (_source != null)
            {
                action(_source);
            }

            return this;
        }

        public Maybe<T> If(Func<T, bool> condition)
        {
            if ((_source != null) && (condition(_source)))
            {
                return this;
            }

            return new Maybe<T>(null);
        }
    }
}
