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

using Rock.ViewModels.Blocks;

namespace Rock.ViewModels.Blocks.Reminders.ReminderList
{
    /// <summary>
    /// Static configuration sent to the Vue layer on page load. Reminder data
    /// and dropdown options are not included here — the frontend calls the
    /// <c>GetReminders</c> block action on mount to populate them so the
    /// initialization payload stays minimal.
    /// </summary>
    public class ReminderListInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current request has an
        /// authenticated person. When false the Vue layer renders a "Please log in"
        /// notification and skips the reminder list entirely.
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the filter affordances should
        /// be visible to the user. Driven by the <c>ShowFilters</c> block setting;
        /// when false the View Options modal and segmented buttons are hidden and
        /// reminders are filtered to the default Active / Due view.
        /// </summary>
        public bool ShowFilters { get; set; }
    }
}
