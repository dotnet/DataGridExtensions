namespace DataGridExtensions.Framework
{
    using System;
    using System.Diagnostics.Contracts;

    internal static class MaybeExtensions
    {
        public static void Do<T>(this T source, Action<T> action)
        {
            Contract.Requires(source != null);
            Contract.Requires(action != null);

            action(source);
        }
    }
}
