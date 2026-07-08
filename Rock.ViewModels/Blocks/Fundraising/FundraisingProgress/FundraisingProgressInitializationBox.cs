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
    /// The initialization box for the Fundraising Progress block.
    /// </summary>
    public class FundraisingProgressInitializationBox
    {
        /// <summary>
        /// Gets or sets the error message that prevents the block from being displayed.
        /// When set, the progress content is hidden and only this message is shown (for
        /// example, when the group is not a fundraising opportunity).
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the title of the fundraising opportunity (the group name).
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the group total header is visible. This is
        /// hidden when viewing a single participant's progress.
        /// </summary>
        public bool IsGroupTotalVisible { get; set; }

        /// <summary>
        /// Gets or sets the combined contribution total for the entire group.
        /// </summary>
        public decimal GroupContributionTotal { get; set; }

        /// <summary>
        /// Gets or sets the combined fundraising goal for the entire group.
        /// </summary>
        public decimal GroupFundraisingGoal { get; set; }

        /// <summary>
        /// Gets or sets the percentage complete for the entire group.
        /// </summary>
        public decimal PercentComplete { get; set; }

        /// <summary>
        /// Gets or sets the per-participant (or per-family) progress rows.
        /// </summary>
        public List<FundraisingProgressBag> ProgressItems { get; set; }
    }
}
