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
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.SystemKey;
using Rock.Utility;

namespace RockWeb.Blocks.Connection
{
    /// <summary>
    /// Block used for Campaign configuration which is also used by job.
    /// </summary>
    [DisplayName( "Campaign Configuration" )]
    [Category( "Connection" )]
    [Description( "Block used for Campaign Connection configuration which is also used by job." )]
    public partial class CampaignConfiguration : Rock.Web.UI.RockBlock
    {
        #region PageParameterKeys

        /// <summary>
        /// A defined list of page parameter keys used by this block.
        /// </summary>
        protected static class PageParameterKey
        {
            /// <summary>
            /// The connection campaign identifier
            /// </summary>
            public const string ConnectionCampaignGuid = "ConnectionCampaignGuid";
        }

        #endregion PageParameterKeys

        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>ConnectionOpportunity
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            dvRequestor.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Person ) ).Id;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                var connectionCampaignGuid = PageParameter( PageParameterKey.ConnectionCampaignGuid ).AsGuidOrNull();

                if ( connectionCampaignGuid.HasValue )
                {
                    ShowDetail( connectionCampaignGuid.Value );
                }
                else
                {
                    pnlDetail.Visible = false;
                }
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlConnectionType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlConnectionType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var selectedConnectionTypeGuid = ddlConnectionType.SelectedValueAsGuid();
            var connectionOpportunityService = new ConnectionOpportunityService( new RockContext() );
            ddlConnectionOpportunity.Items.Clear();
            ddlConnectionOpportunity.Visible = selectedConnectionTypeGuid.HasValue;
            if ( selectedConnectionTypeGuid.HasValue )
            {
                var connectionOpportunities =  connectionOpportunityService.Queryable().Where( a => a.ConnectionType.Guid == selectedConnectionTypeGuid.Value );
                ddlConnectionOpportunity.Items.Add( new ListItem() );
                ddlConnectionOpportunity.Items.AddRange( connectionOpportunities.Select( x => new ListItem { Text = x.Name, Value = x.Guid.ToString() } ).ToArray() );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var campaignConnectionGuid = hfCampaignConnectionGuid.Value.AsGuid();

            CampaignItem campaignConnection = null;
            if ( campaignConnectionGuid == Guid.Empty )
            {
                campaignConnection = new CampaignItem()
                {
                    Guid = Guid.NewGuid()
                };
            }
            else
            {
                campaignConnection = CampaignConnectionHelper.GetCampaignConfiguration( campaignConnectionGuid );
            }

            campaignConnection.Name = tbName.Text;
            campaignConnection.IsActive = cbIsActive.Checked;
            campaignConnection.ConnectionTypeGuid = ddlConnectionType.SelectedValue.AsGuid();
            campaignConnection.RequestCommentsLavaTemplate = ceCommentLavaTemplate.Text;
            campaignConnection.OpportunityGuid = ddlConnectionOpportunity.SelectedValue.AsGuid();
            var dataViewGuid =  new DataViewService( new RockContext() ).GetGuid( dvRequestor.SelectedValue.AsInteger() );
            if ( dataViewGuid.HasValue )
            {
                campaignConnection.DataViewGuid = dataViewGuid.Value;
            }
            campaignConnection.FamilyLimits = rblFamilyLimits.SelectedValueAsEnum<FamilyLimits>( FamilyLimits.Everyone );
            campaignConnection.CreateConnectionRequestOption = rblCreateConnectionRequests.SelectedValueAsEnum<CreateConnectionRequestOptions>( CreateConnectionRequestOptions.AsNeeded );
            if ( gpOptOutGroup.GroupId.HasValue )
            {
                campaignConnection.OptOutGroupGuid = new GroupService( new RockContext() ).GetGuid( gpOptOutGroup.GroupId.Value );
            }
            campaignConnection.DailyLimitAssigned = nbDailyLimit.Text.AsIntegerOrNull();
            campaignConnection.DaysBetweenConnection = nbNumberOfDays.Text.AsInteger();
            campaignConnection.PreferPreviousConnector = cbPreferPreviousConnector.Checked;

            // Save what we have so far, and it will be saved again once the EntitySetId is updated when the thread completes.
            CampaignConnectionHelper.AddOrUpdateCampaignConfiguration( campaignConnection.Guid, campaignConnection );

            // Only update the EntitySet if the campaign is active
            if ( campaignConnection.IsActive )
            {
                // Run this thread in the background since it takes several seconds to calculate.
                Task.Run( () => {
                    campaignConnection.EntitySetId = CampaignConnectionHelper.GetEntitySet( campaignConnection );
                    CampaignConnectionHelper.AddOrUpdateCampaignConfiguration( campaignConnection.Guid, campaignConnection );
                } );
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }


        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="connectionCampaignGuid">The connection campaign identifier.</param>
        public void ShowDetail( Guid connectionCampaignGuid )
        {
            bool readOnly = true;

            CampaignItem connectionCampaign = null;
            if ( !UserCanEdit )
            {
                // User is not authorized
                nbEditModeMessage.Visible = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( "campaign Connection Configuration" );
            }
            else
            {
                nbEditModeMessage.Visible = false;
                readOnly = false;

                if ( connectionCampaignGuid == Guid.Empty )
                {
                    readOnly = false;
                    connectionCampaign = new CampaignItem();
                }
                else
                {
                    var campaignConnectionItems = Rock.Web.SystemSettings.GetValue( CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION ).FromJsonOrNull<List<CampaignItem>>() ?? new List<CampaignItem>();
                    connectionCampaign = campaignConnectionItems.FirstOrDefault( a => a.Guid == connectionCampaignGuid );
                    if ( connectionCampaign != null )
                    {
                        readOnly = false;
                    }
                }
            }

            pnlDetail.Visible = !readOnly;

            if ( !readOnly )
            {
                hfCampaignConnectionGuid.Value = connectionCampaign.Guid.ToString();
                ShowEditDetails( connectionCampaign );
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="connectionCampaignItem">The connectionCampaignItem.</param>
        private void ShowEditDetails( CampaignItem connectionCampaignItem )
        {
            BindControl();

            if ( connectionCampaignItem.Guid == Guid.Empty )
            {
                lTitle.Text = ActionTitle.Add( "Campaign Connection Configuration" ).FormatAsHtmlTitle();
            }
            else
            {
                lTitle.Text = connectionCampaignItem.Name.FormatAsHtmlTitle();
                cbIsActive.Checked = connectionCampaignItem.IsActive;
                ceCommentLavaTemplate.Text = connectionCampaignItem.RequestCommentsLavaTemplate;
                rblFamilyLimits.SetValue( ( int ) connectionCampaignItem.FamilyLimits );
                rblCreateConnectionRequests.SetValue( ( int ) connectionCampaignItem.CreateConnectionRequestOption );
                ShowHideDailyLimit( connectionCampaignItem.CreateConnectionRequestOption );
                nbNumberOfDays.Text = connectionCampaignItem.DaysBetweenConnection.ToStringSafe();
            }

            tbName.Text = connectionCampaignItem.Name;
            ddlConnectionType.SetValue( connectionCampaignItem.ConnectionTypeGuid );
            ddlConnectionType_SelectedIndexChanged( null, null );
            ddlConnectionOpportunity.SetValue( connectionCampaignItem.OpportunityGuid );

            var dataviewId = new DataViewService( new RockContext() ).GetId( connectionCampaignItem.DataViewGuid );
            dvRequestor.SetValue( dataviewId );

            if ( connectionCampaignItem.OptOutGroupGuid.HasValue )
            {
                var groupId = new GroupService( new RockContext() ).GetId( connectionCampaignItem.OptOutGroupGuid.Value );
                gpOptOutGroup.SetValue( groupId );
            }
            else
            {
                gpOptOutGroup.SetValue( null );
            }

            nbDailyLimit.Text = connectionCampaignItem.DailyLimitAssigned.ToStringSafe();
            cbPreferPreviousConnector.Checked = connectionCampaignItem.PreferPreviousConnector;
        }

        /// <summary>
        /// Binds the control
        /// </summary>
        private void BindControl()
        {

            rblFamilyLimits.BindToEnum<FamilyLimits>();
            rblCreateConnectionRequests.BindToEnum<CreateConnectionRequestOptions>();

            var connectionTypeService = new ConnectionTypeService( new RockContext() );
            ddlConnectionType.Items.Add( new ListItem() );
            ddlConnectionType.Items.AddRange( connectionTypeService.Queryable().Select( x => new ListItem { Text = x.Name, Value = x.Guid.ToString() } ).ToArray() );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblCreateConnectionRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblCreateConnectionRequests_SelectedIndexChanged( object sender, EventArgs e )
        {
            RadioButtonList rblCreateConnectionRequests = ( RadioButtonList ) sender;

            ShowHideDailyLimit( rblCreateConnectionRequests.SelectedValueAsEnumOrNull<CreateConnectionRequestOptions>() );
        }

        /// <summary>
        /// Shows the daily limit number box field as needed.
        /// </summary>
        /// <param name="option">The option.</param>
        private void ShowHideDailyLimit( CreateConnectionRequestOptions? option )
        {
            if ( option == CreateConnectionRequestOptions.AllAtOnce )
            {
                nbDailyLimit.Visible = false;
            }
            else
            {
                nbDailyLimit.Visible = true;
            }
        }

        #endregion
    }
}