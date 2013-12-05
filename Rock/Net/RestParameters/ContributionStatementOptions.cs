using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Net.RestParameters
{
    /// <summary>
    /// 
    /// </summary>
    public class ContributionStatementOptions
    {
        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the account ids.
        /// </summary>
        /// <value>
        /// The account ids.
        /// </value>
        public List<int> AccountIds { get; set; }

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [order by zip code].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [order by zip code]; otherwise, <c>false</c>.
        /// </value>
        public bool OrderByZipCode { get; set; }
    }
}
