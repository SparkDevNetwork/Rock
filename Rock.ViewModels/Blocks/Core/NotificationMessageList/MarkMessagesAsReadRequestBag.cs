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

namespace Rock.ViewModels.Blocks.Core.NotificationMessageList
{
    /// <summary>
    /// Describes the request sent to the MarkMessagesAsRead block action.
    /// </summary>
    public class MarkMessagesAsReadRequestBag
    {
        /// <summary>
        /// Gets or sets the identifier keys of the notification messages.
        /// </summary>
        /// <value>The identifier keys of the notification messages.</value>
        public List<string> IdKeys { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to mark the messages as read.
        /// </summary>
        /// <value><c>true</c> if the messages should be marked as read; otherwise, <c>false</c>.</value>
        public bool IsRead { get; set; }
    }
}
