namespace DataGridExtensions.Framework
{
    using System;
    using System.Diagnostics.Contracts;

    using JetBrains.Annotations;

    internal static class MaybeExtensions
    {
        public static void Do<T>([NotNull] this T source, [NotNull] Action<T> action)
        {
            Contract.Requires(source != null);
            Contract.Requires(action != null);

            action(source);
        }
    }
}
