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
using System.Data.Entity;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Security.AuthScopeList;
using Rock.Web.Cache;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Displays a list of auth scopes.
    /// </summary>
    [DisplayName( "OpenID Connect Scopes" )]
    [Category( "Security > OIDC" )]
    [Description( "Block for displaying and editing available OpenID Connect scopes." )]
    [IconCssClass( "fa fa-list" )]
    //[SupportedSiteTypes( SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the auth scope details.",
        Key = AttributeKey.DetailPage )]

    [SystemGuid.EntityTypeGuid( "a4f6030a-c5a9-44f8-abb2-22df2fcb7d91" )]
    [SystemGuid.BlockTypeGuid( "9ff39411-d9ce-4a5d-b04a-2db169a688f4" )]
    [CustomizedGrid]
    public class AuthScopeList : RockEntityListBlockType<AuthScope>
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

        public static class PageParameterKey
        {
            public const string ScopeDetailId = "ScopeId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<AuthScopeListOptionsBag>();
            var builder = GetGridBuilder();
            var isAddDeleteEnabled = GetIsAddEnabled();

            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
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
        private AuthScopeListOptionsBag GetBoxOptions()
        {
            var options = new AuthScopeListOptionsBag();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.ScopeDetailId, "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<AuthScope> GetListQueryable( RockContext rockContext )
        {
            var authClientService = new AuthScopeService( rockContext );
            var authScopeQuery = authClientService.Queryable().AsNoTracking();
            return authScopeQuery;
        }

        /// <inheritdoc/>
        protected override IQueryable<AuthScope> GetOrderedListQueryable( IQueryable<AuthScope> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<AuthScope> GetGridBuilder()
        {
            return new GridBuilder<AuthScope>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "publicName", a => a.PublicName )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "isSystem", a => a.IsSystem );
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
            var entityService = new AuthScopeService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{AuthScope.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {AuthScope.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}
