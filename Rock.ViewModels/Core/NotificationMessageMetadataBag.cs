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

namespace Rock.ViewModels.Core
{
    /// <summary>
    /// The standard metadata that will be used to display the notification.
    /// </summary>
    public class NotificationMessageMetadataBag
    {
        /// <summary>
        /// <para>
        /// Gets or sets the photo URL. This is used to represent the source of
        /// the notification. For example, a notification that originates from
        /// an action a person took might use the person's photo.
        /// </para>
        /// <para>
        /// The image should either be a full absolute URL including the scheme
        /// and hostname or should be a path only URL that begins with <c>~</c>.
        /// If it is a path that begins with <c>~</c> then the URL will be
        /// automatically resolved to the proper scheme and hostname.
        /// </para>
        /// <para>
        /// The image must be square and should be around 128x128 pixels in size.
        /// </para>
        /// </summary>
        /// <value>The photo URL.</value>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class. This is used in some places to
        /// give a visual representation to the type of message.
        /// </summary>
        /// <value>The icon CSS class.</value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the base color. This is used to calculate the background
        /// and foreground colors used when displaying certain parts of the
        /// notification message.
        /// </summary>
        /// <value>The base color.</value>
        public string Color { get; set; }
    }
}
