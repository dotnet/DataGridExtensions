namespace DataGridExtensions
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Text.RegularExpressions;

    /// <summary>
    /// A content filter using the content as a regular expression to match the string representation of the value.
    /// </summary>
    public class RegexContentFilter : IContentFilter
    {
        readonly Regex _filterRegex;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexContentFilter"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="regexOptions">The regex options.</param>
        public RegexContentFilter(string expression, RegexOptions regexOptions)
        {
            Contract.Requires(expression != null);

            try
            {
                _filterRegex = new Regex(expression, regexOptions);
            }
            catch (ArgumentException)
            {
                // invalid user input, just go with a null expression.
            }
        }

        #region IColumnFilter Members

        /// <summary>
        /// Determines whether the specified value matches the condition of this filter.
        /// </summary>
        /// <param name="value">The content.</param>
        /// <returns>
        ///   <c>true</c> if the specified value matches the condition; otherwise, <c>false</c>.
        /// </returns>
        public bool IsMatch(object value)
        {
            if (_filterRegex == null)
                return true;
            if (value == null)
                return false;

            return _filterRegex.IsMatch(value.ToString());
        }

        #endregion
    }

    /// <summary>
    /// Factory to create a <see cref="RegexContentFilter"/>
    /// </summary>
    public class RegexContentFilterFactory : IContentFilterFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegexContentFilterFactory"/> class.
        /// </summary>
        public RegexContentFilterFactory()
            : this(RegexOptions.IgnoreCase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexContentFilterFactory"/> class.
        /// </summary>
        /// <param name="regexOptions">The regex options.</param>
        public RegexContentFilterFactory(RegexOptions regexOptions)
        {
            RegexOptions = regexOptions;
        }

        /// <summary>
        /// Gets or sets the regex options.
        /// </summary>
        RegexOptions RegexOptions 
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

            return new RegexContentFilter(content.ToString(), RegexOptions);
        }

        #endregion
    }
}
