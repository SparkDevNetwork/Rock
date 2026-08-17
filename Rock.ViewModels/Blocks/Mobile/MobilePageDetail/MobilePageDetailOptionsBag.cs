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

namespace Rock.ViewModels.Blocks.Mobile.MobilePageDetail
{
    /// <summary>
    /// The additional configuration options for the Mobile Page Detail block.
    /// </summary>
    public class MobilePageDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the name of the mobile application (site) the page belongs to.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person can deploy the
        /// mobile application. Deploy acts on the whole application, so this reflects
        /// the site's edit authorization rather than the individual page's.
        /// </summary>
        public bool CanDeploy { get; set; }

        /// <summary>
        /// Gets or sets the pages belonging to the mobile application's site,
        /// used to populate the page picker dropdown in the panel header.
        /// </summary>
        public List<ListItemBag> PageItems { get; set; }

        /// <summary>
        /// Gets or sets the layouts available on the mobile application's site,
        /// used to populate the Layout field in the edit panel.
        /// </summary>
        public List<ListItemBag> LayoutItems { get; set; }

        /// <summary>
        /// Gets or sets the values for the Show In Navigation field in the edit panel.
        /// </summary>
        public List<ListItemBag> DisplayInNavWhenItems { get; set; }

        /// <summary>
        /// Gets or sets the values for the Page Type field in the edit panel.
        /// </summary>
        public List<ListItemBag> PageTypeItems { get; set; }

        /// <summary>
        /// Gets or sets the friendly deploy text ("Last Deploy: 5 minutes ago")
        /// for the application, shown as a label in the panel header. Null when
        /// the application has never been deployed.
        /// </summary>
        public string LastDeployText { get; set; }

        /// <summary>
        /// Gets or sets the long-form, locale-friendly tooltip shown on hover of
        /// the deploy label (e.g. "Sunday, March 2, 2025 5:42 PM"). Null when the
        /// application has never been deployed.
        /// </summary>
        public string LastDeployTooltip { get; set; }
    }
}
