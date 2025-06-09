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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.LavaEndpointList;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of endpoints in a lava application.
    /// </summary>

    [DisplayName( "Lava Endpoint List" )]
    [Category( "CMS" )]
    [Description( "Displays a list of endpoints in a lava application." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the lava endpoint details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "B643984C-03C3-46E2-AA41-5E658DE79921" )]
    [Rock.SystemGuid.BlockTypeGuid( "3BA03384-027C-4EE8-B44E-5643D583686D" )]
    [CustomizedGrid]
    public class LavaEndpointList : RockEntityListBlockType<LavaEndpoint>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string LavaApplicationId = "LavaApplicationId";
        }

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<LavaEndpointListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled( GetLavaApplicationId() );
            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private LavaEndpointListOptionsBag GetBoxOptions()
        {
            var applicationId = GetLavaApplicationId();
            var options = new LavaEndpointListOptionsBag();
            if ( applicationId.HasValue )
            {
                var lavaApplicationService = new LavaApplicationService( new RockContext() ).Queryable().FirstOrDefault( a => a.Id == applicationId.Value );
                options.IsBlockVisible = lavaApplicationService != null;
            }
            
            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled( int? applicationId )
        {
            var lavaApplication = LavaApplicationCache.Get( applicationId.HasValue ? applicationId.Value : 0 );

            // If we're adding a new application it won't get exist.
            if ( lavaApplication == null )
            {
                return true;
            }

            return lavaApplication.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var lavaApplicatonId = GetLavaApplicationId();
            var queryParams = new Dictionary<string, string>
            {
                ["LavaEndpointId"] = "((Key))",
                ["LavaApplicationId"] = lavaApplicatonId.HasValue ? lavaApplicatonId.Value.ToString() : "0",
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, queryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<LavaEndpoint> GetListQueryable( RockContext rockContext )
        {
            int? lavaApplicationId = GetLavaApplicationId();
            var qry = base.GetListQueryable( rockContext );
            if( lavaApplicationId.HasValue )
            {
                qry = qry.Where( a => a.LavaApplicationId == lavaApplicationId.Value );
            }

            return qry;
        }

        /// <inheritdoc/>
        protected override GridBuilder<LavaEndpoint> GetGridBuilder()
        {
            var blockOptions = new GridBuilderGridOptions<LavaEndpoint>
            {
                LavaObject = row => row
            };

            return new GridBuilder<LavaEndpoint>()
                .WithBlock( this, blockOptions )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "isSystem", a => a.IsSystem )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "slug", a => a.Slug )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "httpMethod", a => a.HttpMethod )
                .AddField( "securityMode", a => a.SecurityMode )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <summary>
        /// Gets the lava application identifier.
        /// </summary>
        /// <returns></returns>
        private int? GetLavaApplicationId()
        {
            var lavaApplicationIdParam = PageParameter( PageParameterKey.LavaApplicationId );
            var lavaApplicationId = lavaApplicationIdParam.AsIntegerOrNull() ?? Rock.Utility.IdHasher.Instance.GetId( lavaApplicationIdParam );
            return lavaApplicationId;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new LavaEndpointService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{LavaEndpoint.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${LavaEndpoint.FriendlyTypeName}." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk();
            }
        }
    }

    #endregion
}
