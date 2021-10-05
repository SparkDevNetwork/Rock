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
using Rock.Model;
using Rock.Security;
using Rock.Utility.Enums;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Block for displaying and editing available OpenID Connect clients.
    /// </summary>
    [DisplayName( "Rock Security Settings" )]
    [Category( "Security" )]
    [Description( "Block for displaying and editing Rock's security settings." )]
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
                LoadRoleDropdownList();
            }

            ShowHideWarning();
            nbSaveResult.Visible = true;

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the UserLogins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            var highProfile = RoleCache.Get( ddlHighRoles.SelectedValue.AsInteger() );
            var extremeProfile = RoleCache.Get( ddlExtremeRoles.SelectedValue.AsInteger() );

            if ( highProfile == null || extremeProfile == null )
            {
                return;
            }

            _securitySettingsService.SecuritySettings.AccountProtectionProfilesForDuplicateDetectionToIgnore =
                cblIgnoredAccountProtectionProfiles.SelectedValuesAsInt.Select( a => ( AccountProtectionProfile ) a ).ToList();

            _securitySettingsService.SecuritySettings.DisableTokensForAccountProtectionProfiles =
                cblDisableTokensForAccountProtectionProfiles.SelectedValuesAsInt.Select( a => ( AccountProtectionProfile ) a ).ToList();

            _securitySettingsService.SecuritySettings.AccountProtectionProfileSecurityGroup.AddOrReplace( AccountProtectionProfile.Extreme, extremeProfile );
            _securitySettingsService.SecuritySettings.AccountProtectionProfileSecurityGroup.AddOrReplace( AccountProtectionProfile.High, highProfile );

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
        private void BindAccountProtectionProfileChecklistBoxes()
        {
            cblIgnoredAccountProtectionProfiles.Items.Clear();
            cblDisableTokensForAccountProtectionProfiles.Items.Clear();

            foreach ( AccountProtectionProfile item in Enum.GetValues( typeof( AccountProtectionProfile ) ) )
            {
                cblIgnoredAccountProtectionProfiles.Items.Add( new ListItem( item.ConvertToString(), item.ConvertToInt().ToString() ) );
                cblDisableTokensForAccountProtectionProfiles.Items.Add( new ListItem( item.ConvertToString(), item.ConvertToInt().ToString() ) );
            }

        }

        private void BindRoleDropdownList()
        {
            BindRoleDropdownList( ddlHighRoles );
            BindRoleDropdownList( ddlExtremeRoles );
        }

        private void BindRoleDropdownList( RockDropDownList rockDropdownList )
        {
            rockDropdownList.Items.Clear();

            var securityRoleType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid() );

            foreach ( var role in RoleCache.AllRoles() )
            {
                string name = role.IsSecurityTypeGroup ? role.Name : "GROUP - " + role.Name;
                rockDropdownList.Items.Add( new ListItem( name, role.Id.ToString() ) );
            }
        }

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
        }

        private void LoadRoleDropdownList()
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

        private void ShowHideWarning()
        {
            var isExtremeChecked = _securitySettingsService.SecuritySettings.AccountProtectionProfilesForDuplicateDetectionToIgnore.Contains( AccountProtectionProfile.Extreme );
            var isHighChecked = _securitySettingsService.SecuritySettings.AccountProtectionProfilesForDuplicateDetectionToIgnore.Contains( AccountProtectionProfile.High );
            if ( !isExtremeChecked || !isHighChecked )
            {

            }
        }
        #endregion


    }
}