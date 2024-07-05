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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
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
        /// The passwordless sign in session duration default value.
        /// </summary>
        public static readonly int PasswordlessSignInSessionDurationDefaultValue = 120;

        /// <summary>
        /// The passwordless sign in daily ip throttle default value.
        /// </summary>
        public static readonly int PasswordlessSignInDailyIpThrottleDefaultValue = 1000;

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
        /// Gets or sets the disable passwordless sign in for account protection profiles.
        /// </summary>
        /// <value>The disable passwordless sign in for account protection profiles.</value>
        [JsonProperty( ObjectCreationHandling = ObjectCreationHandling.Replace )] // Enables JSON deserialization to replace the default property value assigned in the constructor with a new list instance.
        public List<AccountProtectionProfile> DisablePasswordlessSignInForAccountProtectionProfiles { get; set; }

        /// <summary>
        /// Gets or sets the disable tokens for account protection profiles.
        /// </summary>
        /// <value>
        /// The disable tokens for account protection profiles.
        /// </value>
        public List<AccountProtectionProfile> DisableTokensForAccountProtectionProfiles { get; set; }

        /// <summary>
        /// Gets or sets the account protection profiles that require two-factor authentication.
        /// </summary>
        /// <value>
        /// The account protection profiles that require two-factor authentication.
        /// </value>
        public List<AccountProtectionProfile> RequireTwoFactorAuthenticationForAccountProtectionProfiles { get; set; }

        /// <summary>
        /// Gets or sets the passwordless sign in daily IP throttle.
        /// </summary>
        /// <value>The passwordless sign in daily IP throttle.</value>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Passwordless Sign In Daily IP Throttle must be greater than zero" )]
        public int PasswordlessSignInDailyIpThrottle { get; set; }

        /// <summary>
        /// Gets or sets the passwordless confirmation communication template identifier.
        /// </summary>
        /// <value>The passwordless confirmation communication template identifier.</value>
        [Required]
        public Guid PasswordlessConfirmationCommunicationTemplateGuid { get; set; }

        /// <summary>
        /// Gets or sets the duration of the passwordless sign in session in minutes.
        /// </summary>
        /// <value>The duration of the passwordless sign in session in minutes.</value>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Passwordless Session Duration must be greater than zero" )]
        public int PasswordlessSignInSessionDuration { get; set; }

        /// <summary>
        /// Gets or sets the toggle option to disable predictable ids for get file.
        /// </summary>
        /// <value>The toggle option to disable predictable ids for GetFile, GetImage, and GetAvatar endpoints.</value>
        public bool DisablePredictableIds { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecuritySettings"/> class.
        /// </summary>
        public SecuritySettings()
        {
            AccountProtectionProfilesForDuplicateDetectionToIgnore = new List<AccountProtectionProfile>();
            AccountProtectionProfileSecurityGroup = new Dictionary<AccountProtectionProfile, RoleCache>();
            DisableTokensForAccountProtectionProfiles = new List<AccountProtectionProfile>();
            RequireTwoFactorAuthenticationForAccountProtectionProfiles = new List<AccountProtectionProfile>();
        }
    }
}
