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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility.Enums;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// </summary>
    [DisplayName( "Rock Security Settings" )]
    [Category( "Security" )]
    [Description( "Block for displaying and editing Rock's security settings." )]
    [Rock.SystemGuid.BlockTypeGuid( "186490CD-4132-43BD-9BDF-DD04C6CD2432" )]
    public partial class RockSecuritySettings : RockBlock
    {
        private SecuritySettingsService _securitySettingsService = new SecuritySettingsService();

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            BlockUpdated += Block_BlockUpdated;

            base.OnInit( e );

            BindAccountProtectionProfileChecklistBoxes();
            BindRoleDropdownList();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                LoadSecuritySettings();
                LoadRoleDropdownLists();
                LoadAuthenticationSettings();
            }

            nbSaveResult.Visible = true;

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // no block settings (yet) so nothing needed here
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var highProfile = RoleCache.Get( ddlHighRoles.SelectedValue.AsInteger() );
            var extremeProfile = RoleCache.Get( ddlExtremeRoles.SelectedValue.AsInteger() );

            if ( highProfile == null || extremeProfile == null )
            {
                return;
            }

            _securitySettingsService.SecuritySettings.DisablePredictableIds = cbDisablePredictableIds.Checked;

            _securitySettingsService.SecuritySettings.AccountProtectionProfilesForDuplicateDetectionToIgnore =
                cblIgnoredAccountProtectionProfiles.SelectedValuesAsInt.Select( a => ( AccountProtectionProfile ) a ).ToList();

            _securitySettingsService.SecuritySettings.DisableTokensForAccountProtectionProfiles =
                cblDisableTokensForAccountProtectionProfiles.SelectedValuesAsInt.Select( a => ( AccountProtectionProfile ) a ).ToList();

            if ( IsTwoFactorAuthenticationSupported() )
            {
                _securitySettingsService.SecuritySettings.RequireTwoFactorAuthenticationForAccountProtectionProfiles =
                    cblRequireTwoFactorAuthenticationForAccountProtectionProfiles.SelectedValuesAsInt.Select( a => ( AccountProtectionProfile ) a ).ToList();
            }
            else
            {
                _securitySettingsService.SecuritySettings.RequireTwoFactorAuthenticationForAccountProtectionProfiles = new System.Collections.Generic.List<AccountProtectionProfile>();
            }

            _securitySettingsService.SecuritySettings.AccountProtectionProfileSecurityGroup.AddOrReplace( AccountProtectionProfile.Extreme, extremeProfile );
            _securitySettingsService.SecuritySettings.AccountProtectionProfileSecurityGroup.AddOrReplace( AccountProtectionProfile.High, highProfile );

            _securitySettingsService.SecuritySettings.DisablePasswordlessSignInForAccountProtectionProfiles =
                cblDisablePasswordlessSignInForAccountProtectionProfiles.SelectedValuesAsInt.Select( a => ( AccountProtectionProfile ) a ).ToList();

            _securitySettingsService.SecuritySettings.PasswordlessConfirmationCommunicationTemplateGuid = ddlPasswordlessConfirmationCommunicationTemplate.SelectedValueAsGuid() ?? default;

            if ( !nbPasswordlessSignInDailyIpThrottle.Text.AsIntegerOrNull().HasValue )
            {
                nbPasswordlessSignInDailyIpThrottle.Text = SecuritySettings.PasswordlessSignInDailyIpThrottleDefaultValue.ToString();
            }

            _securitySettingsService.SecuritySettings.PasswordlessSignInDailyIpThrottle = nbPasswordlessSignInDailyIpThrottle.Text.AsInteger();

            if ( !nbPasswordlessSignInSessionDuration.Text.AsIntegerOrNull().HasValue )
            {
                nbPasswordlessSignInSessionDuration.Text = SecuritySettings.PasswordlessSignInSessionDurationDefaultValue.ToString();
            }

            _securitySettingsService.SecuritySettings.PasswordlessSignInSessionDuration = nbPasswordlessSignInSessionDuration.Text.AsInteger();

            _securitySettingsService.SecuritySettings.RejectAuthenticationCookiesIssuedBefore = dtpRejectAuthenticationCookiesIssuedBefore.SelectedDateTime;

            if ( _securitySettingsService.Save() )
            {
                nbSaveResult.Text = "Your Security Settings have been saved.";
                nbSaveResult.NotificationBoxType = NotificationBoxType.Success;
                nbSaveResult.Visible = true;
            }
            else
            {
                var errors = "<li>" + _securitySettingsService.ValidationResults.Select( r => r.ErrorMessage ).JoinStrings( "</li><li>" ) + "</li>";
                errors = errors.Replace( "<li></li>", string.Empty );

                nbSaveResult.Text = $"The following errors occurred while trying to save:<ul>{errors}</ul>";
                nbSaveResult.NotificationBoxType = NotificationBoxType.Danger;
                nbSaveResult.Visible = true;
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Binds the account protection profile checklist boxes.
        /// </summary>
        private void BindAccountProtectionProfileChecklistBoxes()
        {
            cblIgnoredAccountProtectionProfiles.Items.Clear();
            cblDisableTokensForAccountProtectionProfiles.Items.Clear();
            cblRequireTwoFactorAuthenticationForAccountProtectionProfiles.Items.Clear();

            foreach ( AccountProtectionProfile item in Enum.GetValues( typeof( AccountProtectionProfile ) ) )
            {
                cblIgnoredAccountProtectionProfiles.Items.Add( new ListItem( item.ConvertToString(), item.ConvertToInt().ToString() ) );
                cblDisableTokensForAccountProtectionProfiles.Items.Add( new ListItem( item.ConvertToString(), item.ConvertToInt().ToString() ) );
                cblRequireTwoFactorAuthenticationForAccountProtectionProfiles.Items.Add( new ListItem( item.ConvertToString(), item.ConvertToInt().ToString() ) );
                cblDisablePasswordlessSignInForAccountProtectionProfiles.Items.Add( new ListItem( item.ConvertToString(), item.ConvertToInt().ToString() ) );
            }
        }

        /// <summary>
        /// Binds the role dropdown list.
        /// </summary>
        private void BindRoleDropdownList()
        {
            BindRoleDropdownList( ddlHighRoles );
            BindRoleDropdownList( ddlExtremeRoles );
        }

        /// <summary>
        /// Binds the role dropdown list.
        /// </summary>
        /// <param name="rockDropdownList">The rock dropdown list.</param>
        private void BindRoleDropdownList( RockDropDownList rockDropdownList )
        {
            rockDropdownList.Items.Clear();

            foreach ( var role in RoleCache.AllRoles() )
            {
                string name = role.IsSecurityTypeGroup ? role.Name : "GROUP - " + role.Name;
                rockDropdownList.Items.Add( new ListItem( name, role.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Determines whether two-factor authentication is supported.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if two-factor authentication is supported; otherwise, <c>false</c>.
        /// </returns>
        private bool IsTwoFactorAuthenticationSupported()
        {
            var obsidianLoginBlockTypeId = BlockTypeCache.GetId( "5437C991-536D-4D9C-BE58-CBDB59D1BBB3".AsGuid() );

            if ( obsidianLoginBlockTypeId.HasValue )
            {
                var blockService = new BlockService( new RockContext() );
                return blockService.GetByBlockTypeId( obsidianLoginBlockTypeId.Value ).Any();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Loads the security settings.
        /// </summary>
        private void LoadSecuritySettings()
        {
            cblIgnoredAccountProtectionProfiles.SetValues(
                _securitySettingsService
                    .SecuritySettings
                    .AccountProtectionProfilesForDuplicateDetectionToIgnore
                    .Select( a => a.ConvertToInt().ToString() ) );

            cblDisableTokensForAccountProtectionProfiles.SetValues(
                _securitySettingsService
                    .SecuritySettings
                    .DisableTokensForAccountProtectionProfiles
                    .Select( a => a.ConvertToInt().ToString() ) );

            cbDisablePredictableIds.Checked = _securitySettingsService.SecuritySettings.DisablePredictableIds;

            // Clear the 2FA settings when it is not supported.
            if ( !IsTwoFactorAuthenticationSupported() )
            {
                nbTwoFactorAuthenticationDisabled.Visible = true;
                cblRequireTwoFactorAuthenticationForAccountProtectionProfiles.ClearSelection();
                cblRequireTwoFactorAuthenticationForAccountProtectionProfiles.Enabled = false;
            }
            else
            {
                nbTwoFactorAuthenticationDisabled.Visible = false;
                cblRequireTwoFactorAuthenticationForAccountProtectionProfiles.Enabled = true;

                cblRequireTwoFactorAuthenticationForAccountProtectionProfiles.SetValues(
                    _securitySettingsService
                        .SecuritySettings
                        .RequireTwoFactorAuthenticationForAccountProtectionProfiles
                        .Select( a => a.ConvertToInt().ToString() ) );
            }
        }

        /// <summary>
        /// Loads the role dropdown lists.
        /// </summary>
        private void LoadRoleDropdownLists()
        {
            RoleCache highRole = null;
            RoleCache extremeRole = null;

            _securitySettingsService.SecuritySettings.AccountProtectionProfileSecurityGroup.TryGetValue( AccountProtectionProfile.High, out highRole );
            _securitySettingsService.SecuritySettings.AccountProtectionProfileSecurityGroup.TryGetValue( AccountProtectionProfile.Extreme, out extremeRole );

            if ( highRole != null )
            {
                ddlHighRoles.SelectedValue = highRole.Id.ToString();
            }

            if ( extremeRole != null )
            {
                ddlExtremeRoles.SelectedValue = extremeRole.Id.ToString();
            }
        }

        /// <summary>
        /// Loads the authentication settings.
        /// </summary>
        private void LoadAuthenticationSettings()
        {
            nbPasswordlessSignInDailyIpThrottle.Text = _securitySettingsService.SecuritySettings.PasswordlessSignInDailyIpThrottle.ToString();
            nbPasswordlessSignInSessionDuration.Text = _securitySettingsService.SecuritySettings.PasswordlessSignInSessionDuration.ToString();
            
            cblDisablePasswordlessSignInForAccountProtectionProfiles.SetValues(
                _securitySettingsService
                    .SecuritySettings
                    .DisablePasswordlessSignInForAccountProtectionProfiles
                    .Select( a => a.ConvertToInt().ToString() ) );
            
            var communicationTemplates = new SystemCommunicationService( new RockContext() ).Queryable().ToList();
            ddlPasswordlessConfirmationCommunicationTemplate.DataSource = communicationTemplates;
            ddlPasswordlessConfirmationCommunicationTemplate.DataBind();
            ddlPasswordlessConfirmationCommunicationTemplate.SetValue( _securitySettingsService.SecuritySettings.PasswordlessConfirmationCommunicationTemplateGuid );

            dtpRejectAuthenticationCookiesIssuedBefore.SelectedDateTime = _securitySettingsService.SecuritySettings.RejectAuthenticationCookiesIssuedBefore;
        }

        #endregion
    }
}