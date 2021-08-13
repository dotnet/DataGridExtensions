namespace DataGridExtensions
{
    using System;

    /// <summary>
    /// A content filter using a simple "contains" string comparison to match the content and the value.
    /// </summary>
    public class SimpleContentFilter : IContentFilter
    {
        private readonly string _content;
        private readonly StringComparison _stringComparison;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleContentFilter"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="stringComparison">The string comparison.</param>
        public SimpleContentFilter(string content, StringComparison stringComparison)
        {
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
        public bool IsMatch(object? value)
        {
            if (value == null)
                return false;

            // ReSharper disable once ConstantConditionalAccessQualifier => net5.0 
            return value.ToString()?.IndexOf(_content, _stringComparison) >= 0;
        }

        #endregion
    }

    /// <summary>
    /// Factory to create a <see cref="SimpleContentFilter"/>
    /// </summary>
    public class SimpleContentFilterFactory : IContentFilterFactory
    {
        /// <summary>
        /// The default instance.
        /// </summary>
        public static readonly IContentFilterFactory Default = new SimpleContentFilterFactory();

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
        public IContentFilter Create(object? content)
        {
            return new SimpleContentFilter(content?.ToString() ?? string.Empty, StringComparison);
        }

        #endregion
    }
}
