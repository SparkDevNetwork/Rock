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

using Rock.Utility.Enums;

namespace Rock.ViewModels.Blocks.Security.RockSecuritySettings
{
    /// <summary>
    /// The current Rock security settings values, sent to and returned from the client.
    /// </summary>
    public class RockSecuritySettingsBag
    {
        /// <summary>
        /// Gets or sets the account protection profiles for which duplicate detection is disabled.
        /// Individuals with a profile in this list will always create new records (they will not
        /// match existing records during duplicate detection).
        /// </summary>
        public List<AccountProtectionProfile> AccountProtectionProfilesForDuplicateDetectionToIgnore { get; set; }

        /// <summary>
        /// Gets or sets the account protection profiles for which personal impersonation tokens
        /// and authentication tokens are disabled.
        /// </summary>
        public List<AccountProtectionProfile> DisableTokensForAccountProtectionProfiles { get; set; }

        /// <summary>
        /// Gets or sets the account protection profiles that require two-factor authentication
        /// when logging in.
        /// </summary>
        public List<AccountProtectionProfile> RequireTwoFactorAuthenticationForAccountProtectionProfiles { get; set; }

        /// <summary>
        /// Gets or sets the account protection profiles for which passwordless sign-in is disabled.
        /// </summary>
        public List<AccountProtectionProfile> DisablePasswordlessSignInForAccountProtectionProfiles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether predictable IDs are disabled for the GetFile,
        /// GetImage, and GetAvatar endpoints. When <c>true</c>, IdKeys and GUIDs are used instead.
        /// </summary>
        public bool DisablePredictableIds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether server-side model validation is enabled.
        /// Rock must be restarted for changes to this setting to take effect.
        /// </summary>
        public bool EnableServerModelValidation { get; set; }

        /// <summary>
        /// Gets or sets the ID of the security role that is allowed to merge records with an
        /// Account Protection Profile of High.
        /// </summary>
        public int? HighRoleId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the security role that is allowed to merge records with an
        /// Account Protection Profile of Extreme.
        /// </summary>
        public int? ExtremeRoleId { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of passwordless sign-in attempts allowed from a single
        /// IP address within a single day.
        /// </summary>
        public int PasswordlessSignInDailyIpThrottle { get; set; }

        /// <summary>
        /// Gets or sets the duration in minutes that a passwordless sign-in session remains valid.
        /// </summary>
        public int PasswordlessSignInSessionDuration { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the system communication template used to send passwordless
        /// sign-in confirmation messages.
        /// </summary>
        public Guid PasswordlessConfirmationCommunicationTemplateGuid { get; set; }

        /// <summary>
        /// Gets or sets the date and time before which authentication cookies are considered invalid.
        /// When set, any cookie issued before this value will be rejected.
        /// </summary>
        public DateTime? RejectAuthenticationCookiesIssuedBefore { get; set; }

        /// <summary>
        /// Gets or sets the message displayed when a person attempts to sign in via passwordless
        /// sign-in but their protection profile has passwordless sign-in disabled.
        /// </summary>
        public string MessageForDisabledPasswordlessSignIn { get; set; }
    }
}
