using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntry
{
    /// <summary>
    /// Bag containing the information for retrieving communication entry recipients.
    /// </summary>
    public class CommunicationEntryGetRecipientsRequestBag
    {
        /// <summary>
        /// Gets or sets the person alias unique identifiers for which to retrieve recipient data.
        /// </summary>
        /// <value>
        /// The person alias unique identifiers for which to retrieve recipient data.
        /// </value>
        public List<Guid> PersonAliasGuids { get; set; }
    }
}
