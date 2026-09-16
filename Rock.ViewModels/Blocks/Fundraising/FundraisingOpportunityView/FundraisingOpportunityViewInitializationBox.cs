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

namespace Rock.ViewModels.Blocks.Fundraising.FundraisingOpportunityView
{
    /// <summary>
    /// The initialization box for the Fundraising Opportunity View block.
    /// </summary>
    public class FundraisingOpportunityViewInitializationBox
    {
        /// <summary>
        /// Gets or sets the error message that prevents the block from being displayed. When
        /// set, the opportunity content is hidden and only this message is shown (for example,
        /// when the group could not be found).
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the resolved Sidebar Lava Template HTML displayed in the left sidebar.
        /// </summary>
        public string SidebarHtml { get; set; }

        /// <summary>
        /// Gets or sets the resolved Summary Lava Template HTML displayed at the top of the main panel.
        /// </summary>
        public string SummaryHtml { get; set; }

        /// <summary>
        /// Gets or sets the resolved Participant Lava Template HTML (participant actions and progress bar).
        /// </summary>
        public string ParticipantActionsHtml { get; set; }

        /// <summary>
        /// Gets or sets the opportunity details HTML shown on the Details tab.
        /// </summary>
        public string DetailsHtml { get; set; }

        /// <summary>
        /// Gets or sets the resolved Updates Lava Template HTML (content channel items).
        /// </summary>
        public string UpdatesHtml { get; set; }

        /// <summary>
        /// Gets or sets the URL of the opportunity photo displayed in the left sidebar.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the CSS class applied to the opportunity photo.
        /// </summary>
        public string ImageCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the participant actions panel is visible
        /// (the current person is a member of the opportunity group).
        /// </summary>
        public bool IsParticipantActionsVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "Donate to a Participant" button is visible.
        /// </summary>
        public bool IsDonateToParticipantVisible { get; set; }

        /// <summary>
        /// Gets or sets the text shown on the "Donate to a Participant" button.
        /// </summary>
        public string DonateToParticipantButtonText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "Leader Toolbox" button is visible
        /// (the current person has a leader role in the opportunity group).
        /// </summary>
        public bool IsLeaderToolboxVisible { get; set; }

        /// <summary>
        /// Gets or sets the display label for the Details tab (e.g. "Trip Details").
        /// </summary>
        public string DetailsTabLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Updates tab is visible.
        /// </summary>
        public bool IsUpdatesTabVisible { get; set; }

        /// <summary>
        /// Gets or sets the display label for the Updates tab (e.g. "Trip Updates (5)"). Only
        /// set when <see cref="IsUpdatesTabVisible"/> is <c>true</c>.
        /// </summary>
        public string UpdatesTabLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Comments tab is visible.
        /// </summary>
        public bool IsCommentsTabVisible { get; set; }

        /// <summary>
        /// Gets or sets the display label for the Comments tab (e.g. "Comments (3)").
        /// </summary>
        public string CommentsTabLabel { get; set; }

        /// <summary>
        /// Gets or sets the navigation URLs used by the block.
        /// </summary>
        public Dictionary<string, string> NavigationUrls { get; set; }

        /// <summary>
        /// Gets or sets the URL of the current person's avatar, used when adding a new comment
        /// on the Comments tab.
        /// </summary>
        public string PersonAvatarUrl { get; set; }
    }
}
