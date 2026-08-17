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

namespace Rock.ViewModels.Blocks.Finance.FundraisingDonationEntry
{
    /// <summary>
    /// The runtime data used to initialize the Fundraising Donation Entry block.
    /// </summary>
    public class FundraisingDonationEntryBag
    {
        /// <summary>
        /// Gets or sets the URL the browser should immediately navigate to, bypassing
        /// the form. This is set when the participant is already known (supplied in the
        /// query string or resolved by automatic selection).
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the fundraising opportunity is fixed
        /// (supplied via the query string) and therefore shown as read-only text instead
        /// of a selectable list.
        /// </summary>
        public bool IsOpportunityLocked { get; set; }

        /// <summary>
        /// Gets or sets the title of the locked fundraising opportunity.
        /// </summary>
        public string OpportunityTitle { get; set; }

        /// <summary>
        /// Gets or sets the value of the currently selected fundraising opportunity.
        /// </summary>
        public string SelectedOpportunityValue { get; set; }

        /// <summary>
        /// Gets or sets the fundraising opportunities available for selection.
        /// </summary>
        public List<ListItemBag> OpportunityOptions { get; set; }

        /// <summary>
        /// Gets or sets the participants available for the selected fundraising opportunity.
        /// </summary>
        public List<ListItemBag> ParticipantOptions { get; set; }
    }
}
