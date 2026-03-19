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

using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.Constants;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Connection.CampaignConfiguration;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Connection
{
    /// <summary>
    /// Block used for Campaign Connection configuration which is also used by the Campaign Manager job.
    /// </summary>

    [DisplayName( "Campaign Configuration" )]
    [Category( "Connection" )]
    [Description( "Block used for Campaign Connection configuration which is also used by job." )]
    [IconCssClass( "ti ti-plug" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "469C88A7-DB85-4BED-981A-34DF401E13F9" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "57DB2B33-1ABA-49B6-91F2-1266319F2422" )]
    [Rock.SystemGuid.BlockTypeGuid( "9E6C4174-5F2B-4A78-9781-55D7DD209B6C" )]
    public class CampaignConfiguration : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string ConnectionCampaignGuid = "ConnectionCampaignGuid";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<CampaignConfigurationBag, CampaignConfigurationOptionsBag>();

            var campaignGuid = PageParameter( PageParameterKey.ConnectionCampaignGuid ).AsGuidOrNull();
            var campaignItem = LoadCampaignItem( campaignGuid );

            SetBoxInitialState( box, campaignGuid, campaignItem );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( campaignItem );

            return box;
        }

        /// <summary>
        /// Loads the campaign item for the given page-parameter GUID.
        /// Returns null when the GUID is absent or when no matching item exists in system settings.
        /// Returns a brand-new <see cref="CampaignItem"/> when the GUID is <see cref="Guid.Empty"/> (new-item flow).
        /// </summary>
        private static CampaignItem LoadCampaignItem( Guid? campaignGuid )
        {
            if ( !campaignGuid.HasValue )
            {
                return null;
            }

            if ( campaignGuid.Value == Guid.Empty )
            {
                return new CampaignItem
                {
                    Guid = Guid.NewGuid(),
                    IsActive = true
                };
            }

            return CampaignConnectionHelper.GetCampaignConfiguration( campaignGuid.Value );
        }

        /// <summary>
        /// Gets the box options required for the component to render the view or edit the item.
        /// </summary>
        /// <param name="campaignItem">The already-loaded campaign item (may be null for a new item).</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private CampaignConfigurationOptionsBag GetBoxOptions( CampaignItem campaignItem )
        {
            var connectionTypeOptions = GetConnectionTypeOptions();
            var connectionOpportunityOptions = new List<ListItemBag>();

            // Pre-load opportunities for the currently selected connection type.
            if ( campaignItem != null && campaignItem.ConnectionTypeGuid != Guid.Empty )
            {
                connectionOpportunityOptions = GetConnectionOpportunityOptions( campaignItem.ConnectionTypeGuid );
            }

            return new CampaignConfigurationOptionsBag
            {
                ConnectionTypeOptions = connectionTypeOptions,
                ConnectionOpportunityOptions = connectionOpportunityOptions,
            };
        }

        /// <summary>
        /// Sets the initial state of the box.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="campaignGuid">The already-resolved campaign GUID from the page parameter.</param>
        /// <param name="campaignItem">The already-loaded campaign item.</param>
        private void SetBoxInitialState( DetailBlockBox<CampaignConfigurationBag, CampaignConfigurationOptionsBag> box, Guid? campaignGuid, CampaignItem campaignItem )
        {
            if ( !campaignGuid.HasValue )
            {
                box.ErrorMessage = "No campaign identifier was provided.";
                return;
            }

            if ( campaignItem == null && campaignGuid.Value != Guid.Empty )
            {
                box.ErrorMessage = "The specified campaign configuration could not be found.";
                return;
            }

            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( !box.IsEditable )
            {
                box.ErrorMessage = EditModeMessage.ReadOnlyEditActionNotAllowed( "campaign Connection Configuration" );
                return;
            }

            // Only populate the entity when the user can edit.
            var isNew = campaignGuid.Value == Guid.Empty;
            box.Entity = GetBagFromItem( campaignItem, isNew );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <summary>
        /// Creates a bag from the given campaign item for display in the block.
        /// Uses cache for DataView and Group lookups to avoid unnecessary DB hits.
        /// </summary>
        /// <param name="campaignItem">The campaign item to convert.</param>
        /// <returns>A <see cref="CampaignConfigurationBag"/> representing the item.</returns>
        private CampaignConfigurationBag GetBagFromItem( CampaignItem campaignItem, bool isNew = false )
        {
            if ( campaignItem == null )
            {
                return null;
            }

            ListItemBag dataViewBag = null;
            if ( campaignItem.DataViewGuid != Guid.Empty )
            {
                var dataViewCache = DataViewCache.Get( campaignItem.DataViewGuid );
                if ( dataViewCache != null )
                {
                    dataViewBag = dataViewCache.ToListItemBag();
                }
            }

            // Resolve the opt-out group display name for initial load.
            ListItemBag optOutGroupBag = null;
            if ( campaignItem.OptOutGroupGuid.HasValue )
            {
                var groupName = new GroupService( RockContext ).Queryable()
                    .Where( g => g.Guid == campaignItem.OptOutGroupGuid.Value )
                    .Select( g => g.Name )
                    .FirstOrDefault();

                if ( groupName != null )
                {
                    optOutGroupBag = new ListItemBag
                    {
                        Value = campaignItem.OptOutGroupGuid.Value.ToString(),
                        Text = groupName
                    };
                }
            }

            return new CampaignConfigurationBag
            {
                Guid = campaignItem.Guid.ToString(),
                Name = campaignItem.Name,
                IsActive = campaignItem.IsActive,
                ConnectionTypeGuid = campaignItem.ConnectionTypeGuid != Guid.Empty ? campaignItem.ConnectionTypeGuid.ToString() : null,
                ConnectionOpportunityGuid = campaignItem.OpportunityGuid != Guid.Empty ? campaignItem.OpportunityGuid.ToString() : null,
                RequestCommentsLavaTemplate = campaignItem.RequestCommentsLavaTemplate,
                DataView = dataViewBag,
                FamilyLimits = isNew ? ( FamilyLimits? ) null : campaignItem.FamilyLimits,
                OptOutGroup = optOutGroupBag,
                CreateConnectionRequestOption = isNew ? ( CreateConnectionRequestOptions? ) null : campaignItem.CreateConnectionRequestOption,
                DailyLimitAssigned = campaignItem.DailyLimitAssigned,
                DaysBetweenConnection = isNew ? ( int? ) null : campaignItem.DaysBetweenConnection,
                PreferPreviousConnector = campaignItem.PreferPreviousConnector
            };
        }

        /// <summary>
        /// Gets the connection type list items from cache, ordered by name.
        /// </summary>
        /// <returns>A list of <see cref="ListItemBag"/> items representing each connection type.</returns>
        private List<ListItemBag> GetConnectionTypeOptions()
        {
            return ConnectionTypeCache.All()
                .OrderBy( ct => ct.Name )
                .ToListItemBagList();
        }

        /// <summary>
        /// Gets the connection opportunity list items for the specified connection type.
        /// </summary>
        /// <param name="connectionTypeGuid">The Guid of the connection type to filter by.</param>
        /// <returns>A list of <see cref="ListItemBag"/> items representing each opportunity.</returns>
        private List<ListItemBag> GetConnectionOpportunityOptions( Guid connectionTypeGuid )
        {
            return new ConnectionOpportunityService( RockContext ).Queryable()
                .Where( o => o.ConnectionType.Guid == connectionTypeGuid )
                .OrderBy( o => o.Name )
                .Select( o => new ListItemBag
                {
                    Value = o.Guid.ToString(),
                    Text = o.Name
                } )
                .ToList();
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the connection opportunities for the specified connection type.
        /// Called when the connection type selection changes.
        /// </summary>
        /// <param name="connectionTypeGuid">The Guid of the selected connection type.</param>
        /// <returns>A list of connection opportunity list items.</returns>
        [BlockAction]
        public BlockActionResult GetConnectionOpportunities( string connectionTypeGuid )
        {
            var guid = connectionTypeGuid.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return ActionOk( new List<ListItemBag>() );
            }

            var items = GetConnectionOpportunityOptions( guid.Value );

            return ActionOk( items );
        }

        /// <summary>
        /// Saves the campaign configuration from the provided bag and redirects
        /// to the parent page upon success.
        /// </summary>
        /// <param name="bag">The bag containing the campaign configuration data to save.</param>
        /// <returns>The URL of the parent page to redirect to, or an error result.</returns>
        [BlockAction]
        public BlockActionResult Save( CampaignConfigurationBag bag )
        {
            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( "Not authorized to edit campaign configuration." );
            }

            if ( bag == null )
            {
                return ActionBadRequest( "Invalid campaign data provided." );
            }

            var campaignGuid = bag.Guid.AsGuidOrNull();

            if ( !campaignGuid.HasValue || campaignGuid.Value == Guid.Empty )
            {
                return ActionBadRequest( "A valid campaign identifier is required." );
            }

            // Load or create the campaign item.
            var campaignItem = CampaignConnectionHelper.GetCampaignConfiguration( campaignGuid.Value )
                ?? new CampaignItem { Guid = campaignGuid.Value };

            campaignItem.Name = bag.Name;
            campaignItem.IsActive = bag.IsActive;
            campaignItem.ConnectionTypeGuid = bag.ConnectionTypeGuid.AsGuid();
            campaignItem.OpportunityGuid = bag.ConnectionOpportunityGuid.AsGuid();
            campaignItem.RequestCommentsLavaTemplate = bag.RequestCommentsLavaTemplate;
            campaignItem.FamilyLimits = bag.FamilyLimits ?? FamilyLimits.Everyone;
            campaignItem.CreateConnectionRequestOption = bag.CreateConnectionRequestOption ?? CreateConnectionRequestOptions.AsNeeded;
            campaignItem.DailyLimitAssigned = bag.DailyLimitAssigned;
            campaignItem.DaysBetweenConnection = bag.DaysBetweenConnection ?? 0;
            campaignItem.PreferPreviousConnector = bag.PreferPreviousConnector;

            // Resolve the data view Guid from the bag's ListItemBag value.
            var dataViewGuid = bag.DataView?.Value?.AsGuidOrNull();
            campaignItem.DataViewGuid = dataViewGuid ?? Guid.Empty;

            // Resolve the opt-out group Guid.
            campaignItem.OptOutGroupGuid = bag.OptOutGroup?.Value?.AsGuidOrNull();

            // Persist the configuration. This is an initial save; the EntitySet will be
            // updated asynchronously if the campaign is active.
            CampaignConnectionHelper.AddOrUpdateCampaignConfiguration( campaignItem.Guid, campaignItem );

            // If the campaign is active, recalculate the EntitySet in the background since
            // this operation can take several seconds.
            if ( campaignItem.IsActive )
            {
                Task.Run( () =>
                {
                    campaignItem.EntitySetId = CampaignConnectionHelper.GetEntitySet( campaignItem );
                    CampaignConnectionHelper.AddOrUpdateCampaignConfiguration( campaignItem.Guid, campaignItem );
                } );
            }

            return ActionOk( this.GetParentPageUrl() );
        }

        #endregion Block Actions
    }
}
