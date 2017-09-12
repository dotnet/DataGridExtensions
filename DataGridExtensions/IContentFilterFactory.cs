using JetBrains.Annotations;

namespace DataGridExtensions
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Interface to be implemented by a content filter factory.
    /// </summary>
    public partial interface IContentFilterFactory
    {
        /// <summary>
        /// Creates the content filter for the specified content.
        /// </summary>
        /// <param name="content">The content to create the filter for.</param>
        /// <returns>The new filter.</returns>
        [NotNull]
        IContentFilter Create(object content);
    }

    #region IContentFilterFactory contract binding
    [ContractClass(typeof(ContentFilterFactoryContract))]
    public partial interface IContentFilterFactory
    {
    }

    [ContractClassFor(typeof(IContentFilterFactory))]
    abstract class ContentFilterFactoryContract : IContentFilterFactory
    {
        public IContentFilter Create(object content)
        {
            Contract.Ensures(Contract.Result<IContentFilter>() != null);

            throw new System.NotImplementedException();
        }
    }
    #endregion
}
