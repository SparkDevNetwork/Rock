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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Mobile.MobileApplicationDetail
{
    /// <summary>
    /// The response returned by the Mobile Application Detail block's
    /// Save action when an existing application is updated. Carries the
    /// refreshed entity bag plus the server-rendered fields whose value
    /// depends on the freshly-saved state, so the view-mode UI can reflect
    /// the changes without a full block reload.
    /// </summary>
    public class MobileApplicationSaveResponseBag
    {
        /// <summary>
        /// Gets or sets the refreshed entity bag wrapped in a
        /// <see cref="ValidPropertiesBox{T}"/>.
        /// </summary>
        public ValidPropertiesBox<MobileApplicationBag> Bag { get; set; }

        /// <summary>
        /// Gets or sets the re-rendered application summary HTML displayed
        /// on the view panel.
        /// </summary>
        public string ApplicationDetailsHtml { get; set; }

        /// <summary>
        /// Gets or sets the URL of the (possibly new) preview thumbnail.
        /// Null when no thumbnail is configured.
        /// </summary>
        public string PreviewThumbnailUrl { get; set; }

        /// <summary>
        /// Gets or sets the comma-separated list of deep link domains shown
        /// in the info banner above the Deep Links grid.
        /// </summary>
        public string DeepLinkDomainsText { get; set; }
    }
}
