//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;

using DotLiquid;

namespace Rock.Communication
{
    /// <summary>
    /// 
    /// </summary>
    public class Recipient
    {
        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        /// <value>
        /// The email address.
        /// </value>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the merge objects.
        /// </summary>
        /// <value>
        /// The merge objects.
        /// </value>
        public List<object> MergeObjects { get; set; }
    }
}