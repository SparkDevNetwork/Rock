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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupDetail
{
    /// <summary>
    /// Per-block options consumed by the Group Detail Vue layer. Carries the edit-mode
    /// supporting data, the visibility flags driving the Group Tools card, the action-button
    /// visibility helpers used by the panel footer, and a few other display flags.
    /// </summary>
    public class GroupDetailOptionsBag
    {
        #region Edit-Mode Supporting Data

        /// <summary>
        /// Gets or sets a value indicating whether the <c>PreventSelectingInactiveCampus</c>
        /// block attribute is set.
        /// </summary>
        public bool PreventSelectingInactiveCampus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <c>LimittoSecurityRoleGroups</c> block
        /// attribute is enabled.
        /// </summary>
        public bool IsLimitedToSecurityRoleGroups { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user is a member of the
        /// GROUP_ADMINISTRATORS system group.
        /// </summary>
        public bool IsCurrentPersonGroupAdministrator { get; set; }

        /// <summary>
        /// Gets or sets the list of GroupTypes the user is allowed to pick on the Add panel's
        /// Group Type dropdown.
        /// </summary>
        public List<ListItemBag> AllowedGroupTypes { get; set; }

        /// <summary>
        /// Gets or sets the list of signature document templates available for the
        /// "Required Signature Document" dropdown.
        /// </summary>
        public List<ListItemBag> SignatureDocumentTemplates { get; set; }

        /// <summary>
        /// Gets or sets the list of system communications in the RSVP Confirmation category,
        /// used when the group type does not pin the value.
        /// </summary>
        public List<ListItemBag> RsvpSystemCommunicationOptions { get; set; }

        #endregion

        #region Group Tools Visibility

        /// <summary>
        /// Gets or sets a value indicating whether the Attendance row renders in the Group Tools card.
        /// </summary>
        public bool IsAttendanceVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Scheduler row renders in the Group Tools card.
        /// </summary>
        public bool IsSchedulerVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the RSVP row renders in the Group Tools card.
        /// </summary>
        public bool IsRsvpVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Placement row renders in the Group Tools card.
        /// </summary>
        public bool IsPlacementVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Interactive Map row renders in the Group Tools card.
        /// </summary>
        public bool IsMapVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the History row renders in the Group Tools card.
        /// </summary>
        public bool IsHistoryVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Fundraising Progress row renders in the Group Tools card.
        /// </summary>
        public bool IsFundraisingVisible { get; set; }

        #endregion

        #region Action-Button Visibility

        /// <summary>
        /// Gets or sets a value indicating whether the Copy button shows in the panel footer.
        /// </summary>
        public bool IsCopyButtonShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Archive button is visible in place of Delete.
        /// </summary>
        public bool IsArchiveVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Delete button is shown in the panel footer.
        /// </summary>
        public bool IsDeleteVisible { get; set; }

        #endregion

        #region Other Display Flags

        /// <summary>
        /// Gets or sets a value indicating whether the tag list renders in the subheader.
        /// </summary>
        public bool IsTagListShown { get; set; }

        /// <summary>
        /// Gets or sets the resolved DefinedValue Guid of the <c>MapStyle</c> block attribute, or
        /// <c>null</c> to fall back to the default Rock map style.
        /// </summary>
        public Guid? MapStyleValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the URL the panel navigates to when the user cancels out of Add mode.
        /// Computed server-side so route-aware URL building and returnUrl validation can use the
        /// page context. An empty value means no destination is configured and the panel stays put.
        /// </summary>
        public string AddModeCancelUrl { get; set; }

        #endregion
    }
}
