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

namespace Rock.ViewModels.Blocks.Tv.RokuApplicationDetail
{
    /// <summary>
    /// Describes the Roku Application
    /// </summary>
    public class RokuApplicationBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets a user defined description/summary  of the Site.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the login Rock.Model.Page page for the site.
        /// </summary>
        public ListItemBag LoginPage { get; set; }

        /// <summary>
        /// Gets or sets the name of the Site. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to log Page Views into the Interaction tables for pages in this site
        /// </summary>
        public bool EnablePageViews { get; set; }

        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets whether or not the site is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the page view retention duration.
        /// </summary>
        public int? PageViewRetentionDuration { get; set; }

        /// <summary>
        /// Whether or not to show the Roku components.
        /// </summary>
        public bool ShowRokuComponents { get; set; }

        /// <summary>
        /// Gets or sets the roku components for the site.
        /// </summary>
        public string RokuComponents { get; set; }
    }
}
