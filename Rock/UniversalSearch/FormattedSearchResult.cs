using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.UniversalSearch
{
    /// <summary>
    /// Formatted search result object
    /// </summary>
    public class FormattedSearchResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is view allowed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is view allowed; otherwise, <c>false</c>.
        /// </value>
        public bool IsViewAllowed { get; set; }

        /// <summary>
        /// Gets or sets the formatted result.
        /// </summary>
        /// <value>
        /// The formatted result.
        /// </value>
        public string FormattedResult { get; set; }
    }
}
