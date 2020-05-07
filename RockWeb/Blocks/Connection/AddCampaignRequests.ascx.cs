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
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;
using Rock.SystemKey;
using Rock.Utility;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.UI;

namespace RockWeb.Blocks.Connection
{
    /// <summary>
    /// </summary>
    [DisplayName( "Add Campaign Requests" )]
    [Category( "Connection" )]
    [Description( "Adds Campaign Connection Requests" )]

    #region Block Attributes

    #endregion Block Attributes
    public partial class AddCampaignRequests : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class AttributeKey
        {
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        protected static class PageParameterKey
        {
        }

        #endregion PageParameterKeys

        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        #endregion

        #region Methods

        #endregion

        /// <summary>
        /// Handles the Click event of the btnCancelCampaignRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelCampaignRequests_Click( object sender, EventArgs e )
        {
            mdAddCampaignRequests.Hide();
        }

        /// <summary>
        /// Handles the Click event of the btnAddCampaignRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddCampaignRequests_Click( object sender, EventArgs e )
        {
            var campaignConnectionItems = Rock.Web.SystemSettings.GetValue( CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION ).FromJsonOrNull<List<CampaignItem>>() ?? new List<CampaignItem>();
            campaignConnectionItems = campaignConnectionItems.Where( c => c.IsActive ).OrderBy( a => a.Name ).ToList();
            var rockContext = new RockContext();

            // limit to campaigns that the current person is a connector in
            campaignConnectionItems = campaignConnectionItems.Where( a => CampaignConnectionHelper.GetConnectorCampusIds( a, CurrentPerson ).Any() ).ToList();

            ddlCampaignConnectionItemsMultiple.Items.Clear();
            foreach ( var campaignConnectionItem in campaignConnectionItems )
            {
                int pendingCount = CampaignConnectionHelper.GetPendingConnectionCount( campaignConnectionItem, CurrentPerson );
                var listItem = new ListItem();
                listItem.Text = string.Format( "{0} ({1} pending connections)", campaignConnectionItem.Name, pendingCount );
                listItem.Value = campaignConnectionItem.Guid.ToString();
                ddlCampaignConnectionItemsMultiple.Items.Add( listItem );
            }

            nbAddConnectionRequestsMessage.Visible = false;
            nbNumberOfRequests.Visible = true;

            if ( campaignConnectionItems.Count() == 0 )
            {
                nbAddConnectionRequestsMessage.Text = "There are no campaigns available for you to request connections for.";
                nbAddConnectionRequestsMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
                nbAddConnectionRequestsMessage.Visible = true;
                ddlCampaignConnectionItemsMultiple.Visible = false;
                lCampaignConnectionItemSingle.Visible = false;
                nbNumberOfRequests.Visible = false;
            }
            else if ( campaignConnectionItems.Count() == 1 )
            {
                var campaignConnectionItem = campaignConnectionItems[0];
                lCampaignConnectionItemSingle.Visible = true;
                int pendingCount = CampaignConnectionHelper.GetPendingConnectionCount( campaignConnectionItem, CurrentPerson );
                lCampaignConnectionItemSingle.Text = string.Format( "{0} ({1} pending connections)", campaignConnectionItem.Name, pendingCount );

                ddlCampaignConnectionItemsMultiple.Visible = false;
            }
            else
            {
                lCampaignConnectionItemSingle.Visible = false;
                ddlCampaignConnectionItemsMultiple.Visible = true;
            }

            if ( campaignConnectionItems.Count > 0 )
            {
                SetDefaultNumberOfRequests( campaignConnectionItems.First().Guid );
            }

            mdAddCampaignRequests.Show();
        }

        /// <summary>
        /// Sets the default number of requests.
        /// </summary>
        /// <param name="selectedCampaignConnectionItemGuid">The selected campaign connection item unique identifier.</param>
        private void SetDefaultNumberOfRequests( Guid? selectedCampaignConnectionItemGuid )
        {
            if ( !selectedCampaignConnectionItemGuid.HasValue )
            {
                // shouldn't happen
                return;
            }

            var campaignConnectionItems = Rock.Web.SystemSettings.GetValue( CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION ).FromJsonOrNull<List<CampaignItem>>() ?? new List<CampaignItem>();
            var selectedCampaignConnectionItem = campaignConnectionItems.Where( a => a.Guid == selectedCampaignConnectionItemGuid.Value ).FirstOrDefault();

            var rockContext = new RockContext();
            var opportunityService = new ConnectionOpportunityService( rockContext );
            IQueryable<ConnectionOpportunityConnectorGroup> opportunityConnecterGroupQuery = opportunityService.Queryable()
                .Where( a => a.Guid == selectedCampaignConnectionItem.OpportunityGuid )
                .SelectMany( a => a.ConnectionOpportunityConnectorGroups );

            int? defaultDailyLimitAssigned = null;

            // Check to see if the group member has any CampaignDailyLimit values defined.
            var currentPersonConnectorGroupMember = opportunityConnecterGroupQuery
                .Select( s => s.ConnectorGroup ).SelectMany( g => g.Members )
                .WhereAttributeValue( rockContext, av => ( av.Attribute.Key == "CampaignDailyLimit" ) && av.ValueAsNumeric > 0 )
                .FirstOrDefault( m => m.PersonId == this.CurrentPersonId.Value );

            if ( currentPersonConnectorGroupMember != null )
            {
                currentPersonConnectorGroupMember.LoadAttributes();
                defaultDailyLimitAssigned = currentPersonConnectorGroupMember.GetAttributeValue( "CampaignDailyLimit" ).AsIntegerOrNull();
            }

            if ( defaultDailyLimitAssigned == null && selectedCampaignConnectionItem.CreateConnectionRequestOption == CreateConnectionRequestOptions.AsNeeded )
            {
                defaultDailyLimitAssigned = selectedCampaignConnectionItem.DailyLimitAssigned;
            }

            var entitySetItemService = new EntitySetItemService( rockContext );
            int pendingCount = CampaignConnectionHelper.GetPendingConnectionCount( selectedCampaignConnectionItem, CurrentPerson );

            if ( pendingCount == 0 )
            {
                nbAddConnectionRequestsMessage.Text = "There are no pending requests remaining.";
                nbAddConnectionRequestsMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Info;
                nbAddConnectionRequestsMessage.Visible = true;
            }
            else if ( pendingCount < defaultDailyLimitAssigned )
            {
                nbAddConnectionRequestsMessage.Text = string.Format( "There are only {0} pending requests remaining.", pendingCount );
                nbAddConnectionRequestsMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Info;
                nbAddConnectionRequestsMessage.Visible = true;
                defaultDailyLimitAssigned = pendingCount;
            }
            else
            {
                nbAddConnectionRequestsMessage.Visible = false;
            }

            nbNumberOfRequests.Text = defaultDailyLimitAssigned.ToString();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAddCampaignRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAddCampaignRequests_SaveClick( object sender, EventArgs e )
        {
            // note if there is only one CampaignConnectionItem, ddlCampaignConnectionItemsMultiple will not be visible, but it is still the selected one, because there is only one
            var selectedCampaignConnectionItemGuid = ddlCampaignConnectionItemsMultiple.SelectedValue.AsGuidOrNull();
            if ( !selectedCampaignConnectionItemGuid.HasValue )
            {
                // shouldn't happen
                return;
            }

            var campaignConnectionItems = Rock.Web.SystemSettings.GetValue( CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION ).FromJsonOrNull<List<CampaignItem>>() ?? new List<CampaignItem>();
            var selectedCampaignConnectionItem = campaignConnectionItems.Where( a => a.Guid == selectedCampaignConnectionItemGuid.Value ).FirstOrDefault();

            if ( selectedCampaignConnectionItem == null )
            {
                // shouldn't happen
                return;
            }

            int numberOfRequests, numberOfRequestsRemaining;
            numberOfRequests = nbNumberOfRequests.Text.AsInteger();
            CampaignConnectionHelper.AddConnectionRequestsForPerson( selectedCampaignConnectionItem, this.CurrentPerson, numberOfRequests, out numberOfRequestsRemaining );

            if ( numberOfRequestsRemaining == numberOfRequests )
            {
                nbAddConnectionRequestsMessage.Text = "Additional Requests are not available as this time.";
                nbAddConnectionRequestsMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Info;
                nbAddConnectionRequestsMessage.Visible = true;
            }

            mdAddCampaignRequests.Hide();
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCampaignConnectionItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCampaignConnectionItem_SelectedIndexChanged( object sender, EventArgs e )
        {
            var selectedCampaignConnectionItemGuid = ddlCampaignConnectionItemsMultiple.SelectedValue.AsGuidOrNull();
            if ( !selectedCampaignConnectionItemGuid.HasValue )
            {
                // shouldn't happen
                return;
            }

            SetDefaultNumberOfRequests( selectedCampaignConnectionItemGuid.Value );
        }
    }
}