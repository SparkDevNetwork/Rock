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

namespace Rock.ViewModels.Blocks.Security.RockSecuritySettings
{
    /// <summary>
    /// The initialization options for the Rock Security Settings block, containing the
    /// reference data needed to populate the form's dropdowns and checkbox lists.
    /// </summary>
    public class RockSecuritySettingsOptionsBag
    {
        /// <summary>
        /// Gets or sets the list of account protection profile levels available for selection
        /// in the checkbox lists. Built from all <see cref="Rock.Utility.Enums.AccountProtectionProfile"/>
        /// enum values.
        /// </summary>
        public List<ListItemBag> AccountProtectionProfiles { get; set; }

        /// <summary>
        /// Gets or sets the list of security roles available for the High and Extreme
        /// merge-permission dropdowns. Non-security-type groups that are flagged as security
        /// roles are prefixed with "GROUP - " to distinguish them from true security roles.
        /// </summary>
        public List<ListItemBag> Roles { get; set; }

        /// <summary>
        /// Gets or sets the list of active system communication templates available for the
        /// passwordless confirmation template dropdown, projected to (GUID, Title) pairs and
        /// ordered by title.
        /// </summary>
        public List<ListItemBag> PasswordlessCommunicationTemplates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether two-factor authentication is supported in
        /// this Rock instance. Two-factor authentication requires at least one page to use the
        /// Obsidian Login block. When <c>false</c>, the two-factor authentication checkbox list
        /// is disabled and any persisted selections are cleared on save.
        /// </summary>
        public bool IsTwoFactorAuthenticationSupported { get; set; }
    }
}
