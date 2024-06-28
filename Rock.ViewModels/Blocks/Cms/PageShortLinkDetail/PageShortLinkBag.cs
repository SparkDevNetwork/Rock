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

namespace Rock.ViewModels.Blocks.CMS.PageShortLinkDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class PageShortLinkBag : EntityBagBase
    {
        /// <summary>
        /// Get the Default Domain URL of the site to be passed to the front end.
        /// </summary>
        public string DefaultDomainURL { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Site that is associated with this PageShortLink.
        /// </summary>
        public ListItemBag Site { get; set; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the link to be copied to the clipboard in the UI when the user clicks the copy button in the view mode.
        /// </summary>
        /// <value>The link to be copied.</value>
        public string CopyLink { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValue of the UTM Source to be passed to the remote device.
        /// </summary>
        public ListItemBag UtmSourceValue { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValue of the UTM Medium to be passed to the remote device.
        /// </summary>
        public ListItemBag UtmMediumValue { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValue of the UTM Campaign to be passed to the remote device.
        /// </summary>
        public ListItemBag UtmCampaignValue { get; set; }

        /// <summary>
        /// Gets or sets the Utm Term to be passed to the remote device.
        /// </summary>
        public string UtmTerm { get; set; }

        /// <summary>
        /// Gets or sets the Utm Content to be passed to the remote device.
        /// </summary>
        public string UtmContent { get; set; }
    }
}
