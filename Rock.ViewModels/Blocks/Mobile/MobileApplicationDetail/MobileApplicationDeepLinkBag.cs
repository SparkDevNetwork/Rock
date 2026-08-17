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

namespace Rock.ViewModels.Blocks.Mobile.MobileApplicationDetail
{
    /// <summary>
    /// Represents one row in the Deep Links grid on the Mobile Application
    /// Detail block. Deep link routes are stored inside the Site's
    /// AdditionalSettings JSON, so the row key is the route's persisted
    /// Guid rather than an entity IdKey.
    /// </summary>
    public class MobileApplicationDeepLinkBag
    {
        /// <summary>
        /// Gets or sets the persisted route Guid (used as the grid row key
        /// and the input to delete and reorder actions).
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// Gets or sets the route segment (with the application's path prefix
        /// applied for display, e.g. "m/users/profile").
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Gets or sets the title of the mobile page that the route opens, or
        /// "No Page" if the page can no longer be resolved.
        /// </summary>
        public string Page { get; set; }

        /// <summary>
        /// Gets or sets the fallback text shown when a deep link cannot be
        /// opened in the application — either the title of the fallback web
        /// page, or the literal fallback URL.
        /// </summary>
        public string Fallback { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the fallback uses a raw
        /// URL (true) or a Rock page (false). Drives the URL badge in the
        /// Fallback column.
        /// </summary>
        public bool IsUrl { get; set; }
    }
}
