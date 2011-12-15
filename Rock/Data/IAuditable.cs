//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.Data
{
    /// <summary>
    /// Represents a model that tracks when the model was created and last updated and by who
    /// </summary>
    public interface IAuditable
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
