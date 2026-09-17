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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Prayer.PrayerCardView
{
    /// <summary>
    /// The configuration options for the Prayer Card View block. These values
    /// come from block settings and do not change while the block is displayed.
    /// </summary>
    public class PrayerCardViewOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the campus filter should be
        /// displayed above the prayer request cards.
        /// </summary>
        public bool IsCampusFilterVisible { get; set; }

        /// <summary>
        /// Gets or sets the campus type unique identifiers that limit which
        /// campuses appear in the campus filter. Empty means no limit.
        /// </summary>
        public List<Guid> CampusTypeFilterGuids { get; set; }

        /// <summary>
        /// Gets or sets the campus status unique identifiers that limit which
        /// campuses appear in the campus filter. Empty means no limit.
        /// </summary>
        public List<Guid> CampusStatusFilterGuids { get; set; }

        /// <summary>
        /// Gets or sets the text shown on a card's Pray button after the
        /// person has prayed for that request.
        /// </summary>
        public string PrayedButtonText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether prayer team members may
        /// flag a request for administrator review.
        /// </summary>
        public bool IsPrayerTeamFlaggingEnabled { get; set; }
    }
}
