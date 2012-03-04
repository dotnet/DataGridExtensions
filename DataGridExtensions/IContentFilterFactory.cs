using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGridExtensions
{
    /// <summary>
    /// Interface to be implemented by a content filter factory.
    /// </summary>
    public interface IContentFilterFactory
    {
        /// <summary>
        /// Creates the content filter for the specified content.
        /// </summary>
        /// <param name="content">The content to create the filter for.</param>
        /// <returns>The new filter.</returns>
        IContentFilter Create(object content);
    }
}
