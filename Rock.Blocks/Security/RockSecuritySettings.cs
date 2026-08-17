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
using System.ComponentModel;
using System.Linq;

using Humanizer;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility.Enums;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Security.RockSecuritySettings;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Block for displaying and editing Rock's global security settings (account protection
    /// profiles, passwordless sign-in tuning, two-factor authentication gating, and the
    /// authentication-cookie rejection cutoff).
    /// </summary>
    [DisplayName( "Rock Security Settings" )]
    [Category( "Security" )]
    [Description( "Block for displaying and editing Rock's security settings." )]
    [IconCssClass( "ti ti-shield-half" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "208087DA-E267-4656-A9CC-362A69E92A26" )]
    // Was [Rock.SystemGuid.BlockTypeGuid( "5B56AE6E-E73E-4D14-AF3E-680B3569CE38" )]
    [Rock.SystemGuid.BlockTypeGuid( "186490CD-4132-43BD-9BDF-DD04C6CD2432" )]
    public class RockSecuritySettings : RockBlockType
    {
        #region Constants

        /// <summary>
        /// The block type GUID of the Obsidian Login block. Two-factor authentication is
        /// only supported when at least one page is using this block type, so its presence
        /// gates the 2FA UI on this block.
        /// </summary>
        private const string ObsidianLoginBlockTypeGuid = "5437C991-536D-4D9C-BE58-CBDB59D1BBB3";

        #endregion Constants

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var service = new SecuritySettingsService();

            return new CustomBlockBox<RockSecuritySettingsBag, RockSecuritySettingsOptionsBag>
            {
                Bag = GetBag( service ),
                Options = GetOptionsBag( service )
            };
        }

        /// <summary>
        /// Builds the bag representing the current persisted Rock security settings.
        /// </summary>
        /// <param name="service">The security settings service whose <see cref="SecuritySettingsService.SecuritySettings"/> will be projected.</param>
        /// <returns>A populated <see cref="RockSecuritySettingsBag"/>.</returns>
        private RockSecuritySettingsBag GetBag( SecuritySettingsService service )
        {
            var settings = service.SecuritySettings;

            RoleCache highRole = null;
            RoleCache extremeRole = null;
            settings.AccountProtectionProfileSecurityGroup.TryGetValue( AccountProtectionProfile.High, out highRole );
            settings.AccountProtectionProfileSecurityGroup.TryGetValue( AccountProtectionProfile.Extreme, out extremeRole );

            // Fall back to the system default template when the persisted value is empty so the
            // dropdown opens with a real selection (mirrors SecuritySettingsService.GetDefaultSecuritySettings).
            var templateGuid = settings.PasswordlessConfirmationCommunicationTemplateGuid;
            if ( templateGuid == Guid.Empty )
            {
                templateGuid = SystemGuid.SystemCommunication.SECURITY_CONFIRM_LOGIN_PASSWORDLESS.AsGuid();
            }

            return new RockSecuritySettingsBag
            {
                AccountProtectionProfilesForDuplicateDetectionToIgnore = settings.AccountProtectionProfilesForDuplicateDetectionToIgnore ?? new List<AccountProtectionProfile>(),
                DisableTokensForAccountProtectionProfiles = settings.DisableTokensForAccountProtectionProfiles ?? new List<AccountProtectionProfile>(),
                RequireTwoFactorAuthenticationForAccountProtectionProfiles = IsTwoFactorAuthenticationSupported()
                    ? settings.RequireTwoFactorAuthenticationForAccountProtectionProfiles ?? new List<AccountProtectionProfile>()
                    : new List<AccountProtectionProfile>(),
                DisablePasswordlessSignInForAccountProtectionProfiles = settings.DisablePasswordlessSignInForAccountProtectionProfiles ?? new List<AccountProtectionProfile>(),
                DisablePredictableIds = settings.DisablePredictableIds,
                EnableServerModelValidation = settings.EnableServerModelValidation,
                HighRoleId = highRole?.Id,
                ExtremeRoleId = extremeRole?.Id,
                PasswordlessSignInDailyIpThrottle = settings.PasswordlessSignInDailyIpThrottle,
                PasswordlessSignInSessionDuration = settings.PasswordlessSignInSessionDuration,
                PasswordlessConfirmationCommunicationTemplateGuid = templateGuid,
                RejectAuthenticationCookiesIssuedBefore = settings.RejectAuthenticationCookiesIssuedBefore,
                MessageForDisabledPasswordlessSignIn = settings.MessageForDisabledPasswordlessSignIn
            };
        }

        /// <summary>
        /// Builds the options bag of reference data needed by the client (enum list, roles,
        /// passwordless communication templates, 2FA support flag).
        /// </summary>
        /// <param name="service">The security settings service. Not currently used but kept for symmetry with <see cref="GetBag(SecuritySettingsService)"/>.</param>
        /// <returns>A populated <see cref="RockSecuritySettingsOptionsBag"/>.</returns>
        private RockSecuritySettingsOptionsBag GetOptionsBag( SecuritySettingsService service )
        {
            return new RockSecuritySettingsOptionsBag
            {
                AccountProtectionProfiles = typeof( AccountProtectionProfile ).ToEnumListItemBag(),
                Roles = GetRoleListItems(),
                PasswordlessCommunicationTemplates = GetPasswordlessCommunicationTemplates(),
                IsTwoFactorAuthenticationSupported = IsTwoFactorAuthenticationSupported()
            };
        }

        /// <summary>
        /// Builds the list of security roles for the High and Extreme merge-permission dropdowns.
        /// Non-security-type groups that are flagged as security roles are prefixed with
        /// "GROUP - " to distinguish them from true security roles (matches the WebForms behavior).
        /// </summary>
        private List<ListItemBag> GetRoleListItems()
        {
            return RoleCache.AllRoles()
                .Select( role => new ListItemBag
                {
                    Text = role.IsSecurityTypeGroup ? role.Name : "GROUP - " + role.Name,
                    Value = role.Id.ToString()
                } )
                .ToList();
        }

        /// <summary>
        /// Returns the active system communication templates projected to (Guid, Title) pairs
        /// ordered by Title. Projection avoids materializing the full entity collection.
        /// </summary>
        private List<ListItemBag> GetPasswordlessCommunicationTemplates()
        {
            return new SystemCommunicationService( RockContext )
                .Queryable()
                .OrderBy( c => c.Title )
                .Select( c => new ListItemBag
                {
                    Value = c.Guid.ToString(),
                    Text = c.Title
                } )
                .ToList();
        }

        /// <summary>
        /// Determines whether two-factor authentication is supported in this Rock instance.
        /// 2FA requires at least one page to be using the Obsidian Login block.
        /// </summary>
        private bool IsTwoFactorAuthenticationSupported()
        {
            var obsidianLoginBlockTypeId = BlockTypeCache.GetId( ObsidianLoginBlockTypeGuid.AsGuid() );

            if ( !obsidianLoginBlockTypeId.HasValue )
            {
                return false;
            }

            return new BlockService( RockContext ).GetByBlockTypeId( obsidianLoginBlockTypeId.Value ).Any();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Persists the provided Rock security settings.
        /// </summary>
        /// <param name="bag">The bag containing the new security settings values.</param>
        /// <returns>
        /// <see cref="BlockActionResult"/> with the success message on success, or a
        /// <c>BadRequest</c> result whose body is the user-facing error message on failure.
        /// </returns>
        [BlockAction]
        public BlockActionResult Save( RockSecuritySettingsBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "No security settings were provided." );
            }

            var highRole = bag.HighRoleId.HasValue ? RoleCache.Get( bag.HighRoleId.Value ) : null;
            var extremeRole = bag.ExtremeRoleId.HasValue ? RoleCache.Get( bag.ExtremeRoleId.Value ) : null;

            if ( highRole == null || extremeRole == null )
            {
                return ActionBadRequest( "A security role must be selected for both the High and Extreme Account Protection Profiles." );
            }

            var requireTwoFactor = bag.RequireTwoFactorAuthenticationForAccountProtectionProfiles ?? new List<AccountProtectionProfile>();
            var disablePasswordless = bag.DisablePasswordlessSignInForAccountProtectionProfiles ?? new List<AccountProtectionProfile>();

            // If a protection profile both requires 2FA and has passwordless sign-in disabled,
            // anyone in that profile would be locked out. Reject the save with the same message
            // the WebForms block used.
            var lockedOutProtectionProfiles = requireTwoFactor.Intersect( disablePasswordless ).ToList();
            if ( lockedOutProtectionProfiles.Count > 0 )
            {
                var messagePrefix = lockedOutProtectionProfiles.Count == 1
                    ? $"{lockedOutProtectionProfiles[0]} account Protection Profile has"
                    : $"{lockedOutProtectionProfiles.Humanize()} account Protection Profiles have";

                return ActionBadRequest( $"{messagePrefix} passwordless sign-in disabled while requiring two-factor authentication. If two-factor authentication (2FA) is enabled without Passwordless login, someone could get locked out." );
            }

            var service = new SecuritySettingsService();
            var settings = service.SecuritySettings;

            settings.DisablePredictableIds = bag.DisablePredictableIds;
            settings.EnableServerModelValidation = bag.EnableServerModelValidation;
            settings.AccountProtectionProfilesForDuplicateDetectionToIgnore = bag.AccountProtectionProfilesForDuplicateDetectionToIgnore ?? new List<AccountProtectionProfile>();
            settings.DisableTokensForAccountProtectionProfiles = bag.DisableTokensForAccountProtectionProfiles ?? new List<AccountProtectionProfile>();

            // Defensive clear when 2FA is unsupported in this instance so a stale persisted list
            // doesn't silently take effect after the supporting Login block is added.
            settings.RequireTwoFactorAuthenticationForAccountProtectionProfiles = IsTwoFactorAuthenticationSupported()
                ? requireTwoFactor
                : new List<AccountProtectionProfile>();

            settings.AccountProtectionProfileSecurityGroup.AddOrReplace( AccountProtectionProfile.Extreme, extremeRole );
            settings.AccountProtectionProfileSecurityGroup.AddOrReplace( AccountProtectionProfile.High, highRole );

            settings.DisablePasswordlessSignInForAccountProtectionProfiles = disablePasswordless;

            // Substitute the system default template when nothing was selected so the persisted
            // value matches what SecuritySettingsService.GetDefaultSecuritySettings would seed.
            var templateGuid = bag.PasswordlessConfirmationCommunicationTemplateGuid;
            if ( templateGuid == Guid.Empty )
            {
                templateGuid = SystemGuid.SystemCommunication.SECURITY_CONFIRM_LOGIN_PASSWORDLESS.AsGuid();
            }

            settings.PasswordlessConfirmationCommunicationTemplateGuid = templateGuid;
            settings.PasswordlessSignInDailyIpThrottle = bag.PasswordlessSignInDailyIpThrottle;
            settings.PasswordlessSignInSessionDuration = bag.PasswordlessSignInSessionDuration;
            settings.RejectAuthenticationCookiesIssuedBefore = bag.RejectAuthenticationCookiesIssuedBefore;
            settings.MessageForDisabledPasswordlessSignIn = bag.MessageForDisabledPasswordlessSignIn;

            if ( service.Save() )
            {
                return ActionOk( "Your Security Settings have been saved." );
            }

            var validationErrors = service.ValidationResults
                .Select( r => r.ErrorMessage )
                .Where( m => !string.IsNullOrWhiteSpace( m ) )
                .ToList();

            var errorMessage = validationErrors.Count > 0
                ? "The following errors occurred while trying to save:\n- " + string.Join( "\n- ", validationErrors )
                : "An error occurred while trying to save the security settings.";

            return ActionBadRequest( errorMessage );
        }

        #endregion Block Actions
    }
}
