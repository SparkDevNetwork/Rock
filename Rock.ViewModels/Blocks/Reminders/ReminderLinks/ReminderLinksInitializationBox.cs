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

namespace Rock.ViewModels.Blocks.Reminders.ReminderLinks
{
    /// <summary>
    /// The initial configuration payload for the Reminder Links chrome block.
    /// Delivered alongside the server-rendered bell icon and consumed by the
    /// Vue component during mount.
    /// </summary>
    public class ReminderLinksInitializationBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block should render. False
        /// when there is no logged-in person — the bell icon is omitted from the
        /// page entirely.
        /// </summary>
        public bool IsBlockVisible { get; set; }

        /// <summary>
        /// Gets or sets the localStorage key used to cache reminder and notification
        /// counts across page navigations. Matches the legacy key exactly so cached
        /// state survives the cutover deploy.
        /// </summary>
        public string CountsLocalStorageKey { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier of the first scoped context
        /// entity on the current page (with Person swapped to PersonAlias). Null
        /// when the page has no supported context entity — causes the client to
        /// hide the Add Reminder menu item by default.
        /// </summary>
        public int? ContextEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Add Reminder menu item
        /// should render. True when a context entity resolves and the current
        /// person is authorized to use at least one reminder type for its entity
        /// type. Pre-computed server-side so the menu item's visibility is
        /// correct on first dropdown open without a round trip.
        /// </summary>
        public bool CanAddReminder { get; set; }

        /// <summary>
        /// Gets or sets the configured linked-page URLs (View Reminders Page,
        /// Edit Reminder Page, View Notifications Page) keyed by the
        /// NavigationUrlKey enum values.
        /// </summary>
        public Dictionary<string, string> NavigationUrls { get; set; }
    }
}
