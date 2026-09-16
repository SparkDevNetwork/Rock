// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Event.CalendarContentChannelItemList
{
    /// <summary>
    /// The result of deleting (or unlinking) a content channel item from an
    /// event item occurrence.
    /// </summary>
    public class CalendarContentChannelItemListDeleteResponseBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the content channel item entity
        /// was deleted (as opposed to only unlinked from this occurrence).
        /// </summary>
        /// <value>
        /// <c>true</c> if the content channel item was deleted; otherwise, <c>false</c>.
        /// </value>
        public bool WasContentItemDeleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item was only unlinked because
        /// it remains linked to other event item occurrences.
        /// </summary>
        /// <value>
        /// <c>true</c> if the item was only unlinked; otherwise, <c>false</c>.
        /// </value>
        public bool WasUnlinkedOnly { get; set; }

        /// <summary>
        /// Gets or sets a message describing the outcome of the operation.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the title of the content channel item that was acted on.
        /// </summary>
        /// <value>
        /// The item title.
        /// </value>
        public string ItemTitle { get; set; }

        /// <summary>
        /// Gets or sets the remaining event item occurrence links when the item
        /// was only unlinked. Each item's text is a friendly description of the
        /// remaining occurrence.
        /// </summary>
        /// <value>
        /// The remaining occurrence links.
        /// </value>
        public List<ListItemBag> RemainingOccurrenceLinks { get; set; }
    }
}
