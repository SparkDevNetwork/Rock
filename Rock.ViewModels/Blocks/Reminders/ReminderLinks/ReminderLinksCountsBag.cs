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

namespace Rock.ViewModels.Blocks.Reminders.ReminderLinks
{
    /// <summary>
    /// The reminder and notification counts used to render the indicator on the bell icon
    /// and the badges on the View Reminders / View Notifications links.
    /// </summary>
    public class ReminderLinksCountsBag
    {
        /// <summary>
        /// Gets or sets the number of active reminders for the current person.
        /// </summary>
        public int Reminders { get; set; }

        /// <summary>
        /// Gets or sets the number of unread notifications for the current person on the current site.
        /// </summary>
        public int Notifications { get; set; }
    }
}
