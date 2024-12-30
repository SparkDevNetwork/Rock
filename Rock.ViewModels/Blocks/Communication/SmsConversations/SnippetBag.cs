using System;
using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.SmsConversations
{
    /// <summary>
    /// The Snippet Bag for SMS Conversations
    /// </summary>
    public class SnippetBag
    {
        /// <summary>
        /// Gets or sets the snippet data.
        /// </summary>
        public ListItemBag Snippet { get; set; }

        /// <summary>
        /// Gets or sets the list of category GUIDs associated with the snippet.
        /// </summary>
        public List<Guid> Categories { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the snippet (e.g., "Personal" or "Shared").
        /// </summary>
        public string SnippetVisibility { get; set; }

    }
}
