//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class BatchSearchValue
    {
        /// <summary>
        /// Gets or sets the date range.
        /// </summary>
        /// <value>
        /// The date range.
        /// </value>
        public DateTime? BatchDate { get; set; }
        
        /// <summary>
        /// Gets or sets the closed value
        /// </summary>
        /// <value>
        /// The Closed value.
        /// </value>
        public BatchStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the title
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }
           
    }
}