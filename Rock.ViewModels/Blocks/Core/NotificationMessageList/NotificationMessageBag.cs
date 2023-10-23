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

namespace Rock.ViewModels.Blocks.Core.NotificationMessageList
{
    /// <summary>
    /// A notification message that should be displayed in the list block.
    /// </summary>
    public class NotificationMessageBag
    {
        /// <summary>
        /// Gets or sets the identifier of the notification message.
        /// </summary>
        /// <value>The identifier of the notification message.</value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the component identifier key.
        /// </summary>
        /// <value>The component identifier key.</value>
        public string ComponentIdKey { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the date time of the message.
        /// </summary>
        /// <value>The date time of the message.</value>
        public DateTimeOffset DateTime { get; set; }

        /// <summary>
        /// Gets or sets the count related to the message.
        /// </summary>
        /// <value>The count related to the message.</value>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this message has been read.
        /// </summary>
        /// <value><c>true</c> if this message has been read; otherwise, <c>false</c>.</value>
        public bool IsRead { get; set; }

        /// <summary>
        /// Gets or sets the photo URL to display with the message.
        /// </summary>
        /// <value>The photo URL to display with the message.</value>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class that represents the message.
        /// </summary>
        /// <value>The icon CSS class that represents the message.</value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the color of the message.
        /// </summary>
        /// <value>The color of the message.</value>
        public string Color { get; set; }
    }
}
