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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.SystemKey;
using Rock.Utility;
using Rock;
using Rock.ViewModels.Blocks;
using System.Data.Entity;
using Rock.Security;
using Rock.ViewModels.Blocks.Engagement.CampaignList;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays a list of campaign connection configurations..
    /// </summary>

    [DisplayName( "Campaign List" )]
    [Category( "Engagement" )]
    [Description( "Block for viewing list of campaign connection configurations." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the campaign details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "68FF1164-17C0-4D30-A937-B2E628CCBFDE" )]
    [Rock.SystemGuid.BlockTypeGuid( "9BD8B4B1-638E-4F35-9593-1E854BDA44DC" )]
    [CustomizedGrid]
    public class CampaignList : RockListBlockType<CampaignConnectionRow>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

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

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<CampaignListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the queue.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private CampaignListOptionsBag GetBoxOptions()
        {
            var options = new CampaignListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.ConnectionCampaignGuid, "((Key))" )
            };
        }

        protected override IQueryable<CampaignConnectionRow> GetListQueryable( RockContext rockContext )
        {
            var midnightToday = RockDateTime.Today.AddDays( 1 );

            // Deserialize the JSON configuration into a list of campaign items
            var campaignConnectionItems = Rock.Web.SystemSettings.GetValue( CampaignConnectionKey.CAMPAIGN_CONNECTION_CONFIGURATION ).FromJsonOrNull<List<CampaignItem>>() ?? new List<CampaignItem>();

            // Get the related opportunity GUIDs and data view GUIDs
            var relatedOpportunityIds = campaignConnectionItems.Select( a => a.OpportunityGuid ).ToList();
            var relatedDataViewIds = campaignConnectionItems.Select( a => a.DataViewGuid ).ToList();

            // Fetch the related opportunities and data views from the database
            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );
            var dataViewService = new DataViewService( rockContext );
            var relatedOpportunities = connectionOpportunityService.GetListByGuids( relatedOpportunityIds );
            var relatedDataViews = dataViewService.GetListByGuids( relatedDataViewIds );

            // Calculate active requests for each opportunity
            var connectionRequestService = new ConnectionRequestService( rockContext );
            var activeOpportunityRequests = relatedOpportunities.ToDictionary(
                opportunity => opportunity.Guid,
                opportunity => connectionRequestService.Queryable().Count(
                    r => r.ConnectionOpportunityId == opportunity.Id &&
                         ( r.ConnectionState == ConnectionState.Active ||
                          ( r.ConnectionState == ConnectionState.FutureFollowUp && r.FollowupDate.HasValue && r.FollowupDate.Value < midnightToday ) )
                )
            );

            // Calculate pending connections for each campaign
            var entitySetItemService = new EntitySetItemService( rockContext );
            var campaignConnectionRows = campaignConnectionItems.Select( campaignConnectionItem => new CampaignConnectionRow
            {
                Guid = campaignConnectionItem.Guid,
                IsActive = campaignConnectionItem.IsActive,
                Name = campaignConnectionItem.Name,
                DataView = relatedDataViews.FirstOrDefault( dv => dv.Guid == campaignConnectionItem.DataViewGuid ),
                ConnectionOpportunity = relatedOpportunities.FirstOrDefault( op => op.Guid == campaignConnectionItem.OpportunityGuid ),
                ActiveRequests = activeOpportunityRequests.GetValueOrDefault( campaignConnectionItem.OpportunityGuid, 0 ),
                PendingConnections = entitySetItemService.Queryable().Count( i => i.EntitySetId == campaignConnectionItem.EntitySetId )
            } );

            var queryableCampaignConnectionRows = campaignConnectionRows.AsQueryable();

            return queryableCampaignConnectionRows;
        }

        protected override GridBuilder<CampaignConnectionRow> GetGridBuilder()
        {
            return new GridBuilder<CampaignConnectionRow>()
                .WithBlock( this )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "dataView", a => a.DataView.Name )
                .AddTextField( "connectionOpportunity", a => a.ConnectionOpportunity.Name )
                .AddField( "activeRequests", a => a.ActiveRequests )
                .AddField( "pendingConnections", a => a.PendingConnections )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "guid", a => a.Guid.ToString() );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete Campaign." );
            }

            var campaignGuid = key.AsGuid();
            var campaignConnectionItem = CampaignConnectionHelper.GetCampaignConfiguration( campaignGuid );

            if ( campaignConnectionItem == null || campaignConnectionItem.EntitySetId == default( int ) )
            {
                return ActionBadRequest( "Campaign not found." );
            }

            using ( var rockContext = new RockContext() )
            {
                var entitySetService = new EntitySetService( rockContext );
                var entitySet = entitySetService.Get( campaignConnectionItem.EntitySetId );

                if ( entitySet == null )
                {
                    return ActionBadRequest( "EntitySet not found." );
                }

                if ( !entitySetService.CanDelete( entitySet, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                var entitySetItemQry = new EntitySetItemService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( i => i.EntitySetId == entitySet.Id );
                rockContext.BulkDelete( entitySetItemQry );
                CampaignConnectionHelper.RemoveCampaignConfiguration( campaignGuid );
                entitySetService.Delete( entitySet );

                rockContext.SaveChanges();
            }

            return ActionOk();
        }

        #endregion Block Actions
    }

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
