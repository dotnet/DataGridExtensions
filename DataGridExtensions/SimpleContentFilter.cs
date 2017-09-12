using JetBrains.Annotations;
using System.Diagnostics;

namespace DataGridExtensions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// A content filter using a simple "contains" string comparison to match the content and the value.
    /// </summary>
    public class SimpleContentFilter : IContentFilter
    {
        [NotNull]
        private readonly string _content;
        private readonly StringComparison _stringComparison;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleContentFilter"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="stringComparison">The string comparison.</param>
        public SimpleContentFilter([NotNull] string content, StringComparison stringComparison)
        {
            Contract.Requires(content != null);

            _content = content;
            _stringComparison = stringComparison;
        }

        #region IFilter Members

        /// <summary>
        /// Determines whether the specified value matches the condition of this filter.
        /// </summary>
        /// <param name="value">The content.</param>
        /// <returns>
        ///   <c>true</c> if the specified value matches the condition; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMatch(object value)
        {
            if (value == null)
                return false;

            return value.ToString().IndexOf(_content, _stringComparison) >= 0;
        }

        #endregion

        [ContractInvariantMethod]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        [Conditional("CONTRACTS_FULL")]
        private void ObjectInvariant()
        {
            Contract.Invariant(_content != null);
        }
    }

    /// <summary>
    /// Factory to create a <see cref="SimpleContentFilter"/>
    /// </summary>
    public class SimpleContentFilterFactory : IContentFilterFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleContentFilterFactory"/> class.
        /// </summary>
        public SimpleContentFilterFactory()
            : this(StringComparison.CurrentCultureIgnoreCase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleContentFilterFactory"/> class.
        /// </summary>
        /// <param name="stringComparison">The string comparison to use.</param>
        public SimpleContentFilterFactory(StringComparison stringComparison)
        {
            StringComparison = stringComparison;
        }

        /// <summary>
        /// Gets or sets the string comparison.
        /// </summary>
        public StringComparison StringComparison
        { 
            get; 
            set; 
        }

        #region IFilterFactory Members

        /// <summary>
        /// Creates the content filter for the specified content.
        /// </summary>
        /// <param name="content">The content to create the filter for.</param>
        /// <returns>
        /// The new filter.
        /// </returns>
        public IContentFilter Create(object content)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            return new SimpleContentFilter(content.ToString(), StringComparison);
        }

        #endregion
    }
}
