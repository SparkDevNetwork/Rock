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

namespace Rock.ViewModels.Blocks.Cms.PageShortLinkDetail
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
        /// The UTM settings to apply to this short link.
        /// </summary>
        public UtmSettingsBag UtmSettings { get; set; }

        /// <summary>
        /// Gets or sets the list of scheduled redirects that have been
        /// configured for this short link.
        /// </summary>
        public List<ScheduledRedirectBag> ScheduledRedirects { get; set; }
    }
}
