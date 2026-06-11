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

using System;
using System.Collections.Generic;

using Rock.Enums.Connection;
using Rock.Model;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents a single entry in the connection request activity feed, which may be a card-style activity or a system-generated status update.
    /// </summary>
    public class ActivityEntryBag
    {
        /// <summary>
        /// Gets or sets the unique key identifying this activity entry.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the type of this activity entry, determining how it is rendered in the feed.
        /// </summary>
        public ActivityEntryType EntryType { get; set; }

        /// <summary>
        /// Gets or sets the date and time this entry was created.
        /// </summary>
        public DateTimeOffset? EntryDateTime { get; set; }

        /// <summary>
        /// Gets or sets the name of the person who created this entry.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the card entry data when this entry is of a card-style type (e.g., an activity or communication).
        /// </summary>
        public CardEntryBag CardEntry { get; set; }

        /// <summary>
        /// Gets or sets the system update data when this entry represents an automated system change (e.g., a status transition).
        /// </summary>
        public SystemUpdateBag SystemUpdate { get; set; }
    }

    /// <summary>
    /// Represents the display data for a card-style activity feed entry, including content, author photo, and editing metadata.
    /// </summary>
    public class CardEntryBag
    {
        /// <summary>
        /// Gets or sets the title of this card entry.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the main content body of this card entry.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the URL of the author's profile photo.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the list of file attachments associated with this card entry.
        /// </summary>
        public List<ListItemBag> Attachments { get; set; }

        /// <summary>
        /// The GUID of the activity type. Populated for Activity entries to support editing.
        /// </summary>
        public string ActivityTypeGuid { get; set; }

        /// <summary>
        /// The Name of the activity type. Populated for Activity entries to support editing.
        /// </summary>
        public string ActivityTypeName { get; set; }

        /// <summary>
        /// Gets or sets whether or not the activity type for this entry is a system activity type. Populated for Activity entries to support editing.
        /// </summary>
        public bool? IsSystemActivityType { get; set; }

        /// <summary>
        /// The GUID of the connector's person alias. Populated for Activity entries to support editing.
        /// </summary>
        public string ConnectorPersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the encrypted identifier key of the connection opportunity associated with this entry.
        /// </summary>
        public string ConnectionOpportunityIdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the connection opportunity associated with this entry.
        /// </summary>
        public string ConnectionOpportunityName { get; set; }

        /// <summary>
        /// Gets or sets the display name of the connection request's current status at the time of this entry.
        /// </summary>
        public string ConnectionRequestStatus { get; set; }

        /// <summary>
        /// Gets or sets the encrypted identifier key of the connection request this entry belongs to.
        /// </summary>
        public string ConnectionRequestIdKey { get; set; }
    }

    /// <summary>
    /// Represents a system-generated update entry in the activity feed, describing a field value change made automatically.
    /// </summary>
    public class SystemUpdateBag
    {
        /// <summary>
        /// Gets or sets the type of system update that occurred.
        /// </summary>
        public SystemUpdateType SystemUpdateType { get; set; }

        /// <summary>
        /// Gets or sets the display value of the field before the system update was applied.
        /// </summary>
        public string PreviousValue { get; set; }

        /// <summary>
        /// Gets or sets the display value of the field after the system update was applied.
        /// </summary>
        public string NewValue { get; set; }
    }
}