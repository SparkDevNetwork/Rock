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

using Rock.ViewModels.Crm;

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.Badges
{
    /// <summary>
    /// Contains all the initial configuration data required to render the
    /// Badges block.
    /// </summary>
    public class BadgesConfigurationBox
    {
        /// <summary>
        /// Gets or sets the person key identifier of the person being viewed.
        /// </summary>
        /// <value>The person key identifier of the person being viewed.</value>
        public string PersonKey { get; set; }

        /// <summary>
        /// Gets or sets the top left rendered badge content.
        /// </summary>
        /// <value>The top left rendered badge content.</value>
        public List<RenderedBadgeBag> TopLeftBadges { get; set; }

        /// <summary>
        /// Gets or sets the top middle rendered badge content.
        /// </summary>
        /// <value>The top middle rendered badge content.</value>
        public List<RenderedBadgeBag> TopMiddleBadges { get; set; }

        /// <summary>
        /// Gets or sets the top right rendered badge content.
        /// </summary>
        /// <value>The top right rendered badge content.</value>
        public List<RenderedBadgeBag> TopRightBadges { get; set; }

        /// <summary>
        /// Gets or sets the bottom left rendered badge content.
        /// </summary>
        /// <value>The bottom left rendered badge content.</value>
        public List<RenderedBadgeBag> BottomLeftBadges { get; set; }

        /// <summary>
        /// Gets or sets the bottom right rendered badge content.
        /// </summary>
        /// <value>The bottom right rendered badge content.</value>
        public List<RenderedBadgeBag> BottomRightBadges { get; set; }
    }
}
