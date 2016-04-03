namespace DataGridExtensions
{
    using System.Windows;

    /// <summary>
    /// A resource locator to override the windows internal mechanism of resource loading, e.g. because dgx is used in a plugin and multiple assemblies with resources might exist.
    /// </summary>
    public interface IResourceLocator
    {
        /// <summary>
        /// Returns the resource for the specified resource key.
        /// </summary>
        /// <param name="target">The target object</param>
        /// <param name="resourceKey">The resource key.</param>
        /// <returns>The resource.</returns>
        object FindResource(FrameworkElement target, object resourceKey);
    }
}