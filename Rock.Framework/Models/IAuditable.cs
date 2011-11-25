using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rock.Models
{
    /// <summary>
    /// Represents a model that tracks who and when a model was created and last updated
    /// </summary>
    internal interface IAuditable
    {
        /// <summary>
        /// Gets the id.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the created by person id.
        /// </summary>
        /// <value>
        /// The created by person id.
        /// </value>
        int? CreatedByPersonId { get; set; }

        /// <summary>
        /// Gets or sets the modified by person id.
        /// </summary>
        /// <value>
        /// The modified by person id.
        /// </value>
        int? ModifiedByPersonId { get; set; }
    }
}
