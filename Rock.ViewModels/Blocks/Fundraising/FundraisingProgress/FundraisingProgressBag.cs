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

namespace Rock.ViewModels.Blocks.Fundraising.FundraisingProgress
{
    /// <summary>
    /// Represents the fundraising progress for a single participant or family within a
    /// fundraising opportunity.
    /// </summary>
    public class FundraisingProgressBag
    {
        /// <summary>
        /// Gets or sets the display title for the progress row (the participant or family name).
        /// </summary>
        public string ProgressTitle { get; set; }

        /// <summary>
        /// Gets or sets the fundraising goal for the participant or family.
        /// </summary>
        public decimal FundraisingGoal { get; set; }

        /// <summary>
        /// Gets or sets the contribution total for the participant or family.
        /// </summary>
        public decimal ContributionTotal { get; set; }

        /// <summary>
        /// Gets or sets the percentage of the goal that has been achieved. This value can
        /// exceed 100. It is <c>null</c> when no fundraising goal has been configured for the
        /// participant, in which case the UI omits the percentage.
        /// </summary>
        public decimal? PercentComplete { get; set; }

        /// <summary>
        /// Gets or sets the participant's avatar URL.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the individual member progress rows that make up this row. This is
        /// only populated for family rows in family participation mode and is <c>null</c> for
        /// individual-mode rows and for the member rows themselves.
        /// </summary>
        public List<FundraisingProgressBag> ChildItems { get; set; }
    }
}
