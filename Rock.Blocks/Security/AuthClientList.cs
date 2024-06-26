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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Security.AuthClientList;
using Rock.Web.Cache;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Displays a list of auth clients.
    /// </summary>
    [DisplayName( "OpenID Connect Clients" )]
    [Category( "Security > OIDC" )]
    [Description( "Block for displaying and editing available OpenID Connect clients." )]
    [IconCssClass( "fa fa-list" )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the auth client details.",
        Key = AttributeKey.DetailPage )]

    [LinkedPage(
        "OpenID Connect Scopes Page",
        Key = AttributeKey.ScopePage,
        Order = 2 )]

    [SystemGuid.EntityTypeGuid( "ffa316a0-0508-4ad8-806b-d636a30386e7" )]
    [SystemGuid.BlockTypeGuid( "53a34d60-31b8-4d22-bc42-e3b669ed152b" )]
    [CustomizedGrid]
    public class AuthClientList : RockEntityListBlockType<AuthClient>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ScopePage = "ScopePage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string ScopePage = "ScopePage";
        }

        public static class PageParameterKey
        {
            public const string AuthClientId = "AuthClientId";
        }

        public static class PreferenceKey
        {
            public const string FilterName = "filter-name";
            public const string FilterActiveStatus = "filter-active-status";
        }

        #endregion Keys

        #region Properties

        protected string FilterName => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterName );

        protected string FilterActiveStatus => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterActiveStatus );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<AuthClientListOptionsBag>();
            var builder = GetGridBuilder();

            var isAddDeleteEnabled = GetIsAddDeleteEnabled();
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
        private AuthClientListOptionsBag GetBoxOptions()
        {
            var options = new AuthClientListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.AuthClientId, "((Key))" ),
                [NavigationUrlKey.ScopePage] = this.GetLinkedPageUrl( AttributeKey.ScopePage )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<AuthClient> GetListQueryable( RockContext rockContext )
        {
            var authClientService = new AuthClientService( rockContext );
            var authClientQuery = authClientService.Queryable().AsNoTracking();

            if ( FilterName.IsNotNullOrWhiteSpace() )
            {
                authClientQuery = authClientQuery.Where( s => s.Name.Contains( FilterName ) );
            }

            if ( FilterActiveStatus.IsNotNullOrWhiteSpace() )
            {
                switch ( FilterActiveStatus )
                {
                    case "active":
                        authClientQuery = authClientQuery.Where( s => s.IsActive );
                        break;
                    case "inactive":
                        authClientQuery = authClientQuery.Where( s => !s.IsActive );
                        break;
                }
            }

            return authClientQuery;
        }

        /// <inheritdoc/>
        protected override IQueryable<AuthClient> GetOrderedListQueryable( IQueryable<AuthClient> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<AuthClient> GetGridBuilder()
        {
            return new GridBuilder<AuthClient>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "clientId", a => a.ClientId )
                .AddField( "isActive", a => a.IsActive );
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
            var entityService = new AuthClientService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{AuthClient.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {AuthClient.FriendlyTypeName}." );
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
