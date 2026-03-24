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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Administration.NotificationList
{
    /// <summary>
    /// Contains the data for a single notification item displayed in the
    /// Notification List block.
    /// </summary>
    public class NotificationItemBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the notification recipient
        /// record. This is used to mark the notification as read.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the title of the notification.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the HTML message content of the notification.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the CSS class for the notification icon.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the classification of the notification. This is used
        /// to determine the alert styling.
        /// </summary>
        public NotificationClassification Classification { get; set; }
    }
}
