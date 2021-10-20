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

using System.Collections.Generic;
using Rock.Utility.Enums;
using Rock.Web.Cache;

namespace Rock.Security
{
    /// <summary>
    /// This class is used to serialize and de-serialize the core_RockSecuritySettings attribute.
    /// </summary>
    public class SecuritySettings
    {
        /// <summary>
        /// Gets or sets the account protection profiles for duplicate detection to ignore.
        /// </summary>
        /// <value>
        /// The account protection profiles for duplicate detection to ignore.
        /// </value>
        /// <remarks>
        /// If the user's account protection profile is in this list then that user will be ignored by the duplicate detection algorithm.
        /// </remarks>
        public List<AccountProtectionProfile> AccountProtectionProfilesForDuplicateDetectionToIgnore { get; set; }

        /// <summary>
        /// Gets or sets the account protection profile security group.
        /// </summary>
        /// <value>
        /// The account protection profile security group.
        /// </value>
        /// <remarks>
        /// This is the list of Security groups required to be able to merge the given account protection profile.
        /// </remarks>
        public Dictionary<AccountProtectionProfile, RoleCache> AccountProtectionProfileSecurityGroup { get; set; }

        /// <summary>
        /// Gets or sets the disable tokens for account protection profiles.
        /// </summary>
        /// <value>
        /// The disable tokens for account protection profiles.
        /// </value>
        public List<AccountProtectionProfile> DisableTokensForAccountProtectionProfiles { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecuritySettings"/> class.
        /// </summary>
        public SecuritySettings()
        {
            AccountProtectionProfilesForDuplicateDetectionToIgnore = new List<AccountProtectionProfile>();
            AccountProtectionProfileSecurityGroup = new Dictionary<AccountProtectionProfile, RoleCache>();
            DisableTokensForAccountProtectionProfiles = new List<AccountProtectionProfile>();
        }
    }
}
