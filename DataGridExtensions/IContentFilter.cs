namespace DataGridExtensions
{
    using JetBrains.Annotations;

    /// <summary>
    /// Interface to be implemented by content filters.
    /// </summary>
    public interface IContentFilter
    {
        /// <summary>
        /// Determines whether the specified value matches the condition of this filter.
        /// </summary>
        /// <param name="value">The content.</param>
        /// <returns>
        ///   <c>true</c> if the specified value matches the condition; otherwise, <c>false</c>.
        /// </returns>
        bool IsMatch([CanBeNull] object value);
    }
}
