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

namespace Rock.ViewModels.Blocks.Administration.NotificationList
{
    /// <summary>
    /// Contains the main data for the Notification List block, including the
    /// list of unread notification items for the current person.
    /// </summary>
    public class NotificationListBag
    {
        /// <summary>
        /// Gets or sets the list of unread notification items to display.
        /// </summary>
        public List<NotificationItemBag> Notifications { get; set; }
    }
}
