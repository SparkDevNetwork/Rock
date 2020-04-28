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
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;
using Rock.SystemKey;
using Rock.Utility;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Connection
{
    /// <summary>
    /// Block for viewing list of campaign configurations.
    /// </summary>
    [DisplayName( "Campaign List" )]
    [Category( "Connection" )]
    [Description( "Block for viewing list of campaign connection configurations." )]

    #region Block Attribute Settings
    [LinkedPage( "Detail Page",
        Order = 0,
        Key = AttributeKey.DetailPage
        )]
    #endregion Block Attribute Settings

    public partial class CampaignList : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

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

        private const string CAMPUS_SETTING = "MyConnectionOpportunities_SelectedCampus";
        DateTime _midnightToday = RockDateTime.Today.AddDays( 1 );

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            bool canEdit = IsUserAuthorized( Authorization.EDIT );

            gCampaigns.DataKeyNames = new string[] { "Guid" };
            gCampaigns.Actions.ShowAdd = canEdit;
            gCampaigns.Actions.AddClick += gCampaigns_AddClick;
            gCampaigns.GridRebind += gCampaigns_GridRebind;
            gCampaigns.IsDeleteEnabled = canEdit;
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
                BindGrid();
            }
        }

        #endregion Control Methods

        #region Events

        /// <summary>
        /// Handles the RowSelected event of the gCampaigns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCampaigns_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, new Dictionary<string, string> { { PageParameterKey.ConnectionCampaignGuid, e.RowKeyValue.ToString() } } );
        }

        /// <summary>
        /// Handles the DeleteClick event of the gCampaigns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCampaigns_DeleteClick( object sender, RowEventArgs e )
        {
            var campaignConnectionItem = CampaignConnectionHelper.GetCampaignConfiguration( e.RowKeyValue.ToString().AsGuid() );
            if ( campaignConnectionItem != null && campaignConnectionItem.EntitySetId != default( int ) )
            {
                var rockContext = new RockContext();
                var entitySetService = new EntitySetService( rockContext );
                var entitySet = entitySetService.Get( campaignConnectionItem.EntitySetId );

                string errorMessage;
                if ( !entitySetService.CanDelete( entitySet, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                var entitySetItemQry = new EntitySetItemService( rockContext )
                       .Queryable().AsNoTracking()
                       .Where( i => i.EntitySetId == entitySet.Id );
                rockContext.BulkDelete( entitySetItemQry );
                entitySetService.Delete( entitySet );
                rockContext.SaveChanges();

                CampaignConnectionHelper.RemoveCampaignConfiguration( e.RowKeyValue.ToString().AsGuid() );

                BindGrid();
            }
        }

        /// <summary>
        /// Handles the AddClick event of the gCampaigns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gCampaigns_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, new Dictionary<string, string> { { PageParameterKey.ConnectionCampaignGuid, Guid.Empty.ToString() } } );
        }

        /// <summary>
        /// Handles the GridRebind event of the gCampaigns control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gCampaigns_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion Events

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();

            var campaignConnectionItems = Rock.Web.SystemSettings.GetValue( CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION ).FromJsonOrNull<List<CampaignItem>>() ?? new List<CampaignItem>();

            var relatedOpportunityIds = campaignConnectionItems.Select( a => a.OpportunityGuid ).ToList();
            List<ConnectionOpportunity> relatedOpportunities = new List<ConnectionOpportunity>();
            Dictionary<Guid, int> activeOpportunityRequests = new Dictionary<Guid, int>();
            if ( relatedOpportunityIds.Any() )
            {
                var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
                var connectionRequestService = new ConnectionRequestService( rockContext );
                var connectionRequestsQry = connectionRequestService.Queryable();
                relatedOpportunities = connectionOpportunityService.GetListByGuids( relatedOpportunityIds );
                foreach ( var connectionOpportunity in relatedOpportunities )
                {
                    var activeRequestCount = connectionRequestsQry
                        .Where( a => a.ConnectionOpportunityId == connectionOpportunity.Id &&
                                    ( a.ConnectionState == ConnectionState.Active ||
                                    ( a.ConnectionState == ConnectionState.FutureFollowUp && a.FollowupDate.HasValue && a.FollowupDate.Value < _midnightToday ) ) )
                        .Count();

                    activeOpportunityRequests.AddOrReplace( connectionOpportunity.Guid, activeRequestCount );
                }
            }

            var relatedDataViewIds = campaignConnectionItems.Select( a => a.DataViewGuid ).ToList();
            List<DataView> relatedDataViews = new List<DataView>();
            if ( relatedDataViewIds.Any() )
            {
                var dataViewService = new DataViewService( rockContext );
                relatedDataViews = dataViewService.GetListByGuids( relatedDataViewIds );
            }

            var campaignConnectionRows = new List<CampaignConnectionRow>();
            var entitySetItemQry = new EntitySetItemService( rockContext ).Queryable();
            foreach ( var campaignConnectionItem in campaignConnectionItems )
            {
                var campaignConnectionRow = new CampaignConnectionRow();
                campaignConnectionRow.Guid = campaignConnectionItem.Guid;
                campaignConnectionRow.Name = campaignConnectionItem.Name;
                campaignConnectionRow.ConnectionOpportunity = relatedOpportunities.FirstOrDefault( a => a.Guid == campaignConnectionItem.OpportunityGuid );
                campaignConnectionRow.DataView = relatedDataViews.FirstOrDefault( a => a.Guid == campaignConnectionItem.DataViewGuid );
                campaignConnectionRow.ActiveRequests = activeOpportunityRequests.GetValueOrNull( campaignConnectionItem.OpportunityGuid ) ?? 0;
                campaignConnectionRow.PendingConnections = entitySetItemQry.Where( a => a.EntitySetId == campaignConnectionItem.EntitySetId ).Count();
                campaignConnectionRow.IsActive = campaignConnectionItem.IsActive;
                campaignConnectionRows.Add( campaignConnectionRow );
            }

            var qry = campaignConnectionRows.AsQueryable();

            SortProperty sortProperty = gCampaigns.SortProperty;
            if ( sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( g => g.Name );
            }

            gCampaigns.DataSource = qry.ToList();
            gCampaigns.DataBind();
        }

        #endregion Internal Methods

        #region Helper Class

        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignConnectionRow"/> class.
        /// </summary>
        public class CampaignConnectionRow
        {
            /// <summary>
            /// Gets or sets the Guid of the campaign connection. 
            /// </summary>
            /// <value>
            /// A <see cref="System.Guid"/> representing the Guid of the campaign connection.
            /// </value>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this campaign connection is active.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this campaign connection is enabled; otherwise, <c>false</c>.
            /// </value>
            public bool IsActive { get; set; }

            /// <summary>
            /// Gets or sets the campaign name.
            /// </summary>
            /// <value>
            /// The campaign name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the dataview.
            /// </summary>
            /// <value>
            /// The dataview.
            /// </value>
            public DataView DataView { get; set; }

            /// <summary>
            /// Gets or sets the opportunity.
            /// </summary>
            /// <value>
            /// The opportunity.
            /// </value>
            public ConnectionOpportunity ConnectionOpportunity { get; set; }

            /// <summary>
            /// Gets or sets the active request count
            /// </summary>
            /// <value>
            /// The active request count
            /// </value>
            public int ActiveRequests { get; set; }

            /// <summary>
            /// Gets or sets the pending connections count
            /// </summary>
            /// <value>
            /// The pending connections count
            /// </value>
            public int PendingConnections { get; set; }
        }

        #endregion Helper Class
    }
}