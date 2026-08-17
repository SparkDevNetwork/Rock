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

namespace Rock.ViewModels.Blocks.Reminders.ReminderEdit
{
    /// <summary>
    /// Contains the configuration options for the Reminder Edit block,
    /// including dropdown data and navigation URLs.
    /// </summary>
    public class ReminderEditOptionsBag
    {
        /// <summary>
        /// Gets or sets the available reminder types for the entity type,
        /// filtered to those the current person can use.
        /// </summary>
        public List<ListItemBag> ReminderTypes { get; set; }

        /// <summary>
        /// Gets or sets the URL to navigate to when the user saves or cancels.
        /// </summary>
        public string ParentPageUrl { get; set; }
    }
}
