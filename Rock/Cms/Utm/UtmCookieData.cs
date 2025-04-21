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
using System;

namespace Rock.Cms.Utm
{
    /// <summary>
    /// Urchin Tracking Module (UTM) data for the current user session, stored in the <seealso cref="UTM_COOKIE_NAME"/> cookie.
    /// </summary>
    public class UtmCookieData
    {
        /// <summary>
        /// The name used to identify this cookie.
        /// </summary>
        public const string UTM_COOKIE_NAME = ".ROCK_UTM_DATA";

        /// <summary>
        /// Describes the origin of traffic to this link, such as a search engine, newsletter, or specific website.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Describes the marketing or advertising medium that directed a user to your site.
        /// </summary>
        public string Medium { get; set; }

        /// <summary>
        /// Tags traffic with a specific campaign name.
        /// </summary>
        public string Campaign { get; set; }

        /// <summary>
        /// The search keywords or terms that are associated with this link.
        /// </summary>
        public string Term { get; set; }

        /// <summary>
        /// Differentiates between links that point to the same URL within the same ad or campaign, such as text or images.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// A flag indicating if any fields contain a value.
        /// </summary>
        public bool IsEmpty()
        {
            var isEmpty = Source.IsNullOrWhiteSpace()
                && Medium.IsNullOrWhiteSpace()
                && Campaign.IsNullOrWhiteSpace()
                && Term.IsNullOrWhiteSpace()
                && Content.IsNullOrWhiteSpace();
            return isEmpty;
        }
    }
}
