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

namespace Rock.AI.Agent.Classes.Skills.SiteSkill
{
    /// <summary>
    /// Lightweight result model for a Rock RMS Site (website, mobile app, or TV app).
    /// </summary>
    internal class SiteResult
    {
        /// <summary>
        /// Stable identifier for the site (used by functions; avoid showing to end users).
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Human-readable name of the site.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Short description of the site’s purpose or audience.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type label, e.g., <c>Website</c>, <c>MobileApp</c>, <c>TvApp</c>.
        /// </summary>
        public string SiteType { get; set; }

        /// <summary>
        /// Optional external URL for the site, if applicable (e.g., public website).
        /// </summary>
        public string ExternalUrl { get; set; }
    }
}
