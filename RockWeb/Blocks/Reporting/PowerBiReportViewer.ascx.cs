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
using System.Web.UI;

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
    [TextField( "GroupId", "The PowerBI GroupId that the report belongs to", true, "", "CustomSetting", 1, "GroupId" )]
    [TextField( "Report URL", "The URL of the report to display.", true, "", "CustomSetting", 2, "ReportUrl" )]
    public partial class PowerBiReportViewer : Rock.Web.UI.RockBlockCustomSettings
    {
        #region Fields

        protected string _accessToken = string.Empty;
        protected string _embedUrl = string.Empty;

        #endregion

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

        #region Events

        /// <summary>
        /// Shows the settings.
        /// </summary>
        protected override void ShowSettings()
        {
            pnlEditModal.Visible = true;
            //pnlView.Visible = false;

            var biAccounts = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.POWERBI_ACCOUNTS.AsGuid() );

            ddlSettingAccountList.DataSource = biAccounts.DefinedValues;
            ddlSettingAccountList.DataTextField = "Value";
            ddlSettingAccountList.DataValueField = "Guid";
            ddlSettingAccountList.DataBind();
            ddlSettingAccountList.Items.Insert( 0, "" );

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "PowerBiAccount" ) ) )
            {
                // check that the power bi account still exists
                var configuredAccount = DefinedValueCache.Read( GetAttributeValue( "PowerBiAccount" ) );

                if ( configuredAccount != null )
                {
                    ddlSettingAccountList.SelectedValue = GetAttributeValue( "PowerBiAccount" );

                    var reportList = GetReportList( GetAttributeValue( "PowerBiAccount" ).AsGuid() );

                    ddlSettingReportUrl.DataSource = reportList;
                    ddlSettingReportUrl.DataTextField = "name";
                    ddlSettingReportUrl.DataValueField = "embedurl";
                    ddlSettingReportUrl.DataBind();

                    ddlSettingReportUrl.Items.Insert( 0, "" );

                    ddlSettingReportUrl.SelectedValue = GetAttributeValue( "ReportUrl" );
                }
            }

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
            SetAttributeValue( "PowerBiAccount", ddlSettingAccountList.SelectedValue );
            SetAttributeValue( "ReportUrl", ddlSettingReportUrl.SelectedValue );
            SaveAttributeValues();

            mdEdit.Hide();
            pnlEditModal.Visible = false;
            upnlContent.Update();

            ShowView();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlSettingAccountList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSettingAccountList_SelectedIndexChanged( object sender, EventArgs e )
        {
            var reportList = GetReportList( ddlSettingAccountList.SelectedValue.AsGuid() );

            ddlSettingReportUrl.DataSource = reportList;
            ddlSettingReportUrl.DataTextField = "name";
            ddlSettingReportUrl.DataValueField = "embedurl";
            ddlSettingReportUrl.DataBind();

            ddlSettingReportUrl.Items.Insert( 0, "" );
        }

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

        #endregion

        #region Methods

        /// <summary>
        /// Gets the report list.
        /// </summary>
        /// <param name="biAccountValueGuid">The bi account value unique identifier.</param>
        /// <returns></returns>
        private List<PBIReport> GetReportList( Guid biAccountValueGuid )
        {
            var accessToken = PowerBiUtilities.GetAccessToken( biAccountValueGuid );
            string debugGroupId = "1d8fd188-d641-49b6-8a2c-6194fa70af1b";
            return PowerBiUtilities.GetReports( accessToken, debugGroupId );
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        private void ShowView()
        {
            pnlEditModal.Visible = false;
            pnlView.Visible = true;

            nbError.Text = string.Empty;

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "ReportUrl" ) ) )
            {
                _embedUrl = GetAttributeValue( "ReportUrl" );
            }
            else
            {
                pnlView.Visible = false;
                nbError.NotificationBoxType = NotificationBoxType.Warning;
                nbError.Text = "No report has been configured.";
                return;
            }

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "PowerBiAccount" ) ) )
            {
                // ensure that the account still exists as a defined value
                var accountValue = DefinedValueCache.Read( GetAttributeValue( "PowerBiAccount" ) );

                if ( accountValue != null )
                {
                    _accessToken = PowerBiUtilities.GetAccessToken( accountValue.Guid );

                    if ( string.IsNullOrWhiteSpace( _accessToken ) )
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