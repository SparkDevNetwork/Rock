using System.Collections.Generic;

namespace Rock.Rest
{
    /// <summary>
    /// The results of a multi-item query including information on how to access
    /// the next page of results.
    /// </summary>
    /// <typeparam name="T">The type of item returned.</typeparam>
    public class PaginatedResult<T>
    {
        /// <summary>
        /// Gets or sets cursor to use when loading the next page of results.
        /// </summary>
        /// <value>
        /// The cursor to use when loading the next page of results.
        /// </value>
        public string NextPage { get; set; }

        /// <summary>
        /// Gets or sets the items loaded in this request.
        /// </summary>
        /// <value>
        /// The items loaded in this request.
        /// </value>
        public IList<T> Items { get; set; }
    }
}
