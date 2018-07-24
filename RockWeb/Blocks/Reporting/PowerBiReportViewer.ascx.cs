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
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Power Bi Report Viewer" )]
    [Category( "Reporting" )]
    [Description( "This block displays a selected report from Power BI." )]

    [DefinedValueField( Rock.SystemGuid.DefinedType.POWERBI_ACCOUNTS, "Power BI Account", "The Power BI account to use to retrieve the report.", true, false, "", "CustomSetting", 0, "PowerBiAccount" )]
    [TextField( "Workspace", "The PowerBI workspace that the report belongs to", true, "", "CustomSetting", 1, key: "GroupId" )]
    [TextField( "Report URL", "The URL of the report to display.", true, "", "CustomSetting", 2, "ReportUrl" )]
    public partial class PowerBiReportViewer : Rock.Web.UI.RockBlockCustomSettings
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.AddConfigurationUpdateTrigger( upnlContent );
            this.BlockUpdated += PowerBiReportViewer_BlockUpdated;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the PowerBiReportViewer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PowerBiReportViewer_BlockUpdated( object sender, EventArgs e )
        {
            ShowView();
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowView();
            }
        }

        #endregion

        #region Block Settings

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlEditModal.Visible = true;

            var biAccounts = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.POWERBI_ACCOUNTS.AsGuid() );

            ddlSettingPowerBiAccount.Items.Clear();
            ddlSettingPowerBiAccount.Items.Add( new ListItem() );
            foreach ( var biAccount in biAccounts.DefinedValues )
            {
                ddlSettingPowerBiAccount.Items.Add( new ListItem( biAccount.Value, biAccount.Guid.ToString() ) );
            }

            var configuredAccountGuid = GetAttributeValue( "PowerBiAccount" ).AsGuidOrNull();
            var configuredGroupId = GetAttributeValue( "GroupId" );

            ddlSettingPowerBiAccount.SetValue( configuredAccountGuid );
            LoadGroupList();

            ddlSettingPowerBiGroup.SetValue( configuredGroupId );
            LoadReportList();

            ddlSettingPowerBiReportUrl.SetValue( GetAttributeValue( "ReportUrl" ) );

            upnlContent.Update();
            mdEdit.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            SetAttributeValue( "PowerBiAccount", ddlSettingPowerBiAccount.SelectedValue );
            SetAttributeValue( "GroupId", ddlSettingPowerBiGroup.SelectedValue );
            SetAttributeValue( "ReportUrl", ddlSettingPowerBiReportUrl.SelectedValue );
            SaveAttributeValues();

            mdEdit.Hide();
            pnlEditModal.Visible = false;
            upnlContent.Update();

            ShowView();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlSettingPowerBiGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSettingPowerBiGroup_SelectedIndexChanged( object sender, EventArgs e )
        {
            LoadReportList();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlSettingPowerBiAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void ddlSettingPowerBiAccount_SelectedIndexChanged( object sender, EventArgs e )
        {
            LoadGroupList();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the Click event of the btnLogin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnLogin_Click( object sender, EventArgs e )
        {
            // Authenticate
            PowerBiUtilities.AuthenticateAccount( GetAttributeValue( "PowerBiAccount" ).AsGuid(), Request.Url.AbsoluteUri );
        }

        /// <summary>
        /// Loads the group list.
        /// </summary>
        private void LoadGroupList()
        {
            var accessToken = PowerBiUtilities.GetAccessToken( ddlSettingPowerBiAccount.SelectedValue.AsGuid() );
            var groups = PowerBiUtilities.GetGroups( accessToken );

            ddlSettingPowerBiGroup.Items.Clear();
            ddlSettingPowerBiGroup.Items.Add( new ListItem() );
            foreach ( var group in groups )
            {
                ddlSettingPowerBiGroup.Items.Add( new ListItem( group.Name, group.Id ) );
            }
        }

        /// <summary>
        /// Loads the report list.
        /// </summary>
        private void LoadReportList()
        {
            var accessToken = PowerBiUtilities.GetAccessToken( ddlSettingPowerBiAccount.SelectedValue.AsGuid() );
            var reports = PowerBiUtilities.GetReports( accessToken, ddlSettingPowerBiGroup.SelectedValue );

            ddlSettingPowerBiReportUrl.Items.Clear();
            ddlSettingPowerBiReportUrl.Items.Add( new ListItem() );
            foreach ( var report in reports )
            {
                ddlSettingPowerBiReportUrl.Items.Add( new ListItem( report.name, report.embedUrl ) );
            }
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        private void ShowView()
        {
            pnlEditModal.Visible = false;
            pnlView.Visible = true;

            nbError.Text = string.Empty;
            hfReportEmbedUrl.Value = GetAttributeValue( "ReportUrl" );

            if ( hfReportEmbedUrl.Value.IsNullOrWhiteSpace() )
            {
                pnlView.Visible = false;
                nbError.NotificationBoxType = NotificationBoxType.Warning;
                nbError.Text = "No report has been configured.";
                return;
            }

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "PowerBiAccount" ) ) )
            {
                // ensure that the account still exists as a defined value
                var accountValue = DefinedValueCache.Get( GetAttributeValue( "PowerBiAccount" ) );

                if ( accountValue != null )
                {
                    hfAccessToken.Value = PowerBiUtilities.GetAccessToken( accountValue.Guid );

                    if ( string.IsNullOrWhiteSpace( hfAccessToken.Value ) )
                    {
                        pnlView.Visible = false;
                        pnlLogin.Visible = true;
                    }
                }
                else
                {
                    pnlView.Visible = false;
                    nbError.NotificationBoxType = NotificationBoxType.Warning;
                    nbError.Text = "The account configured for this block no longer exists.";
                    return;
                }
            }
        }

        #endregion
    }
}