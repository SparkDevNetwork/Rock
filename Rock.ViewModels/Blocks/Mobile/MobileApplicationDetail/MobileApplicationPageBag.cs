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
    /// Represents one row in the Pages grid on the Mobile Application Detail
    /// block.
    /// </summary>
    public class MobileApplicationPageBag
    {
        /// <summary>
        /// Gets or sets the encrypted IdKey of the page.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the internal name of the page.
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of the layout the page uses.
        /// </summary>
        public string LayoutName { get; set; }

        /// <summary>
        /// Gets or sets the display rule for showing the page in navigation
        /// (e.g. Always, When Allowed, Never).
        /// </summary>
        public string DisplayInNavWhen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this page is the default
        /// page for the site. The delete control is suppressed for the
        /// default page because the site requires it to function.
        /// </summary>
        public bool IsDefaultPage { get; set; }
    }
}
