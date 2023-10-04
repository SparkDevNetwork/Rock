// <copyright>
// Copyright by Triumph Tech
//
// NOTICE: All information contained herein is, and remains
// the property of Triumph Tech LLC. The intellectual and technical concepts contained
// herein are proprietary to Triumph Tech LLC  and may be covered by U.S. and Foreign Patents,
// patents in process, and are protected by trade secret or copyright law.
//
// Dissemination of this information or reproduction of this material
// is strictly forbidden unless prior written permission is obtained
// from Triumph Tech LLC.
// </copyright>
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using tech.triumph.Magnus.Classes;
using tech.triumph.Magnus.Utilities;

namespace RockWeb.Plugins.tech_Triumph.Magnus
{
    /// <summary>
    /// Displays the details Typeform settings.
    /// </summary>
    [DisplayName( "Magnus Settings" )]
    [Category( "Triumph Tech > Magnus Editor" )]
    [Description( "Configures the settings for the Magnus Remote Edit plugin." )]
    public partial class MagnusSettings : RockBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowView();
            }

        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shows the view.
        /// </summary>
        private void ShowView()
        {
            pnlEdit.Visible = false;
            pnlView.Visible = true;

            var settings = MagnusHelpers.GetSettings();

            // If settings is null show the edit screen instead of the view.
            if ( settings == null )
            {
                ShowEdit();
                return;
            }

            var leftConfiguration = new DescriptionList();
            var rightConfiguration = new DescriptionList();

            lDetailsRight.Text = "<dl><dt>Allow IP Ranges</dt><dd>";

            var clientIp = MagnusHelpers.GetIpFromContext( Context );

            btnAddCurrentIp.Visible = true;
            foreach ( var ipSetting in settings.IpAddressFilters )
            {
                var matchesRule = MagnusHelpers.IpMatchesFilter( clientIp, ipSetting );

                // If the current IP address has not matched at least one rule show button to easily add current IP
                if ( matchesRule )
                {
                    btnAddCurrentIp.Visible = false;
                }

                lDetailsRight.Text += $"{ipSetting.Key}: {ipSetting.Value} {( matchesRule ? "<span class='badge badge-success'>Matches Rule</span>" : "" )} <br>";
            }         

            lDetailsRight.Text += "</dd></dl>";
            lDetailsRight.Text += rightConfiguration.Html;

            leftConfiguration.Add( "Security Role", RoleCache.Get( settings.AuthorizedSecurityRoleId ).Name );
            leftConfiguration.Add( "Allowed Physical Directories", string.Join( "<br>", settings.AllowedPhysicalDirectories ) );
            leftConfiguration.Add( "Enabled Virtual Filesystems", string.Join( "<br>", MagnusHelpers.GetEnabledVirtualFilesystems().Select( fs => fs.Value.DisplayName ) ) );

            lDetailsLeft.Text = leftConfiguration.Html;

            lCurrentIpAddress.Text = MagnusHelpers.GetIpFromContext( Context );
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        private void ShowEdit()
        {
            pnlEdit.Visible = true;
            pnlView.Visible = false;

            lCurrentIpAddress.Text = MagnusHelpers.GetIpFromContext( Context );

            var settings = MagnusHelpers.GetSettings( true );

            var ipSettingString = string.Empty;

            foreach ( var ipSetting in settings.IpAddressFilters )
            {
                ipSettingString += $"{ipSetting.Key}^{ipSetting.Value}|";
            }
            ipSettingString.TrimEnd( '|' );

            kvlIpAddressSubnets.Value = ipSettingString;

            // Setup the security role edit
            var groupService = new GroupService( new RockContext() );
            var securityRoleGroupTypeGuid = Rock.SystemGuid.GroupType.GROUPTYPE_SECURITY_ROLE.AsGuid();

            var groupList = RoleCache.AllRoles()
                    .Select( r => new
                    {
                        r.Id,
                        r.Name
                    } ).ToList();

            ddlSecurityRole.DataSource = groupList;
            ddlSecurityRole.DataBind();
            ddlSecurityRole.Items.Insert( 0, new ListItem() );

            ddlSecurityRole.SetValue( settings.AuthorizedSecurityRoleId.ToString() );

            // Allowed Physical Direcories
            cblAllowedDirectories.Items.Clear();
            cblAllowedDirectories.Items.Add( "App_Data" );
            cblAllowedDirectories.Items.Add( "Content" );
            cblAllowedDirectories.Items.Add( "Plugins" );
            cblAllowedDirectories.Items.Add( "Themes" );

            cblAllowedDirectories.SetValues( settings.AllowedPhysicalDirectories );

            // Allowed Virtual File Systems
            cblEnabledVirtualFilesystems.Items.Clear();
            cblEnabledVirtualFilesystems.DataSource = MagnusHelpers.GetVirtualFilesystems().Select( v =>  new { v.Value.DisplayName, v.Key } ).ToList();
            cblEnabledVirtualFilesystems.DataValueField = "Key";
            cblEnabledVirtualFilesystems.DataTextField = "DisplayName";
            cblEnabledVirtualFilesystems.DataBind();
            cblEnabledVirtualFilesystems.SetValues( settings.EnabledVirtualFilesystems );

        }

        #endregion

        #region Events
        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var settings = MagnusHelpers.GetSettings( true );


            var kvIpaddresses = kvlIpAddressSubnets.Value.ToKeyValuePairList();
            var ipConfigurationSettings = new List<IpAddressFilter>();

            foreach ( var kvSetting in kvIpaddresses )
            {
                if ( kvSetting.Key.IsNotNullOrWhiteSpace() && kvSetting.Value.ToString().IsNotNullOrWhiteSpace() )
                {
                    ipConfigurationSettings.Add( new IpAddressFilter
                    {
                        Key = kvSetting.Key,
                        Value = kvSetting.Value.ToString()
                    } );
                }
            }

            settings.IpAddressFilters = ipConfigurationSettings;
            settings.AuthorizedSecurityRoleId = ddlSecurityRole.SelectedValue.AsInteger();
            settings.AllowedPhysicalDirectories = cblAllowedDirectories.SelectedValues;
            settings.EnabledVirtualFilesystems = cblEnabledVirtualFilesystems.SelectedValues;

            // Save Settings
            SystemSettings.SetValue( tech.triumph.Magnus.SystemKey.Attribute.MAGNUS_SETTINGS, settings.ToJson() );

            // Reload the filesystem cache
            MagnusHelpers.GetVirtualFilesystems( false );

            ShowView();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            ShowView();
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {

            ShowEdit();

            pnlView.Visible = false;
            pnlEdit.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnAddCurrentIp control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddCurrentIp_Click( object sender, EventArgs e )
        {
            lCurrentIp.Text = MagnusHelpers.GetIpFromContext( Context );

            mbAddIpAddress.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mbAddIpAddress control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mbAddIpAddress_SaveClick( object sender, EventArgs e )
        {
            var settings = MagnusHelpers.GetSettings();

            var currentIp = MagnusHelpers.GetIpFromContext( Context );

            settings.IpAddressFilters.Add( new IpAddressFilter
            {
                Key = tbLocationName.Text,
                Value = currentIp
            } );

            // Save Settings
            SystemSettings.SetValue( tech.triumph.Magnus.SystemKey.Attribute.MAGNUS_SETTINGS, settings.ToJson() );

            mbAddIpAddress.Hide();
            ShowView();
        }
        #endregion
    }
}