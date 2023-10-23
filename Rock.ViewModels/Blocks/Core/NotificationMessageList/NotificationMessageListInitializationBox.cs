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

namespace Rock.ViewModels.Blocks.Core.NotificationMessageList
{
    /// <summary>
    /// Describes the data required to initialize the Obsidian component for
    /// the notification message list block.
    /// </summary>
    public class NotificationMessageListInitializationBox
    {
        /// <summary>
        /// Gets or sets the messages to display upon loading.
        /// </summary>
        /// <value>The messages to display upon loading.</value>
        public List<NotificationMessageBag> Messages { get; set; }

        /// <summary>
        /// Gets or sets the component types used for filtering purposes.
        /// </summary>
        /// <value>The component types used for filtering purposes.</value>
        public List<ListItemBag> ComponentTypes { get; set; }
    }
}
