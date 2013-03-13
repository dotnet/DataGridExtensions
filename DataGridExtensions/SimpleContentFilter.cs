using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGridExtensions
{
    /// <summary>
    /// A content filter using a simple "contains" string comparison to match the content and the value.
    /// </summary>
    public class SimpleContentFilter : IContentFilter
    {
        private readonly string content;
        private readonly StringComparison stringComparison;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleContentFilter"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="stringComparison">The string comparison.</param>
        public SimpleContentFilter(string content, StringComparison stringComparison)
        {
            this.content = content;
            this.stringComparison = stringComparison;
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

            return value.ToString().IndexOf(content, stringComparison) >= 0;
        }

        #endregion
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
            this.StringComparison = stringComparison;
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
