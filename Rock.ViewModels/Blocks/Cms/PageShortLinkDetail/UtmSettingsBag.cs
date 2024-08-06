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

namespace Rock.ViewModels.Blocks.Cms.PageShortLinkDetail
{
    /// <summary>
    /// ShortLink UTM (Urchin Tracking Module) settings.
    /// </summary>
    public class UtmSettingsBag
    {
        /// <summary>
        /// Identifies a UtmSource describing the origin of traffic to this
        /// link, such as a search engine, newsletter, or specific website.
        /// </summary>
        public ListItemBag UtmSource { get; set; }

        /// <summary>
        /// Identifies a UtmMedium Defined Value describing the marketing or
        /// advertising medium that directed a user to your site.
        /// </summary>
        public ListItemBag UtmMedium { get; set; }

        /// <summary>
        /// Identifies a UtmCampaign Defined Value that tags traffic with a
        /// specific campaign name.
        /// </summary>
        public ListItemBag UtmCampaign { get; set; }

        /// <summary>
        /// The search keywords or terms that are associated with this link.
        /// </summary>
        public string UtmTerm { get; set; }

        /// <summary>
        /// Differentiates between links that point to the same URL within the
        /// same ad or campaign, such as text or images.
        /// </summary>
        public string UtmContent { get; set; }
    }
}
