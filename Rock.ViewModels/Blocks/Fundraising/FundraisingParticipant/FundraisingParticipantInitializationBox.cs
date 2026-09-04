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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Fundraising.FundraisingParticipant
{
    /// <summary>
    /// The initialization box for the Fundraising Opportunity Participant block.
    /// </summary>
    public class FundraisingParticipantInitializationBox
    {
        /// <summary>
        /// Gets or sets the error message that prevents the block from being displayed.
        /// When set, the participant content is hidden and only this message is shown (for
        /// example, when the group or participant could not be found).
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the resolved Profile Lava Template HTML displayed at the top of the main panel.
        /// </summary>
        public string ProfileHtml { get; set; }

        /// <summary>
        /// Gets or sets the resolved Progress Lava Template HTML.
        /// </summary>
        public string ProgressHtml { get; set; }

        /// <summary>
        /// Gets or sets the resolved Updates Lava Template HTML (content channel items).
        /// </summary>
        public string UpdatesHtml { get; set; }

        /// <summary>
        /// Gets or sets the resolved Requirements Header Lava Template HTML.
        /// </summary>
        public string RequirementsHeaderHtml { get; set; }

        /// <summary>
        /// Gets or sets the configuration for the group member requirements container.
        /// <c>null</c> when the opportunity has no requirements or the viewer is not authorized.
        /// </summary>
        public FundraisingParticipantRequirementsBag Requirements { get; set; }

        /// <summary>
        /// Gets or sets the profile-completeness tip shown to the participant (for example,
        /// to add a photo or a personal introduction). <c>null</c> when nothing is missing.
        /// </summary>
        public string ProfileWarningText { get; set; }

        /// <summary>
        /// Gets or sets the URL of the opportunity photo displayed in the left sidebar.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the CSS class applied to the opportunity photo.
        /// </summary>
        public string ImageCssClass { get; set; }

        /// <summary>
        /// Gets or sets the text shown on the main page button.
        /// </summary>
        public string MainPageButtonText { get; set; }

        /// <summary>
        /// Gets or sets the header shown above the contributions grid.
        /// </summary>
        public string ContributionsHeader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person may edit the profile.
        /// </summary>
        public bool IsEditProfileVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the clipboard "copy profile link" icon is shown.
        /// </summary>
        public bool IsClipboardIconVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the contributions tab is visible.
        /// </summary>
        public bool IsContributionsTabVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the updates tab is visible.
        /// </summary>
        public bool IsUpdatesTabVisible { get; set; }

        /// <summary>
        /// Gets or sets the display label for the updates tab (e.g. "Trip Updates (5)"). Only
        /// set when <see cref="IsUpdatesTabVisible"/> is <c>true</c>.
        /// </summary>
        public string UpdatesTabLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the amount column is visible in the contributions grid.
        /// </summary>
        public bool IsAmountColumnVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether commenting is enabled for the opportunity.
        /// </summary>
        public bool IsCommentingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the contributions grid definition.
        /// </summary>
        public GridDefinitionBag GridDefinition { get; set; }

        /// <summary>
        /// Gets or sets the navigation URLs used by the block.
        /// </summary>
        public Dictionary<string, string> NavigationUrls { get; set; }
    }
}
