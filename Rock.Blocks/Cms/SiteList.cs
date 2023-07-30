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
using Rock.ViewModels.Blocks;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Demonstrates the various parts of the Obsidian List blocks.
    /// </summary>

    [DisplayName( "Site List" )]
    [Category( "CMS" )]
    [Description( "Displays the list of sites." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes
    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [EnumsField(
        "Site Type",
        "Includes Items with the following Type.",
        typeof( SiteType ),
        false, "",
        order: 1,
        key: AttributeKey.SiteType )]

    [BooleanField( "Show Delete Column",
        Description = "Determines if the delete column should be shown.",
        DefaultBooleanValue = false,
        IsRequired = true,
        Key = AttributeKey.ShowDeleteColumn,
        Order = 2 )]
    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "F3F57167-C120-4166-8D9B-87CA1C6B5169" )]
    [Rock.SystemGuid.BlockTypeGuid( "093913DC-29FB-4261-BEE4-A6EB9EC90DA5" )]
    [CustomizedGrid]
    public class SiteList : RockEntityListBlockType<Site>
    {
        #region Attribute Keys
        private static class AttributeKey
        {
            public const string SiteType = "SiteType";
            public const string ShowDeleteColumn = "ShowDeleteColumn";
            public const string DetailPage = "DetailPage";
        }
        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var builder = GetGridBuilder();

            return new ListBlockBox<Dictionary<string, object>>
            {
                IsAddEnabled = true,
                IsDeleteEnabled = GetAttributeValue(AttributeKey.ShowDeleteColumn).AsBoolean(),
                GridDefinition = builder.BuildDefinition(),
                Options = new Dictionary<string, object>()
                {
                    { AttributeKey.DetailPage, this.GetLinkedPageUrl(AttributeKey.DetailPage) },
                }
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Site> GetListQueryable( RockContext rockContext )
        {
            var siteType = GetAttributeValue( AttributeKey.SiteType ).SplitDelimitedValues().Select( a => a.ConvertToEnumOrNull<SiteType>() ).ToList();

            var qry = base.GetListQueryable( rockContext );
            if ( siteType.Count() > 0 )
            {
                // Filter by block setting Site type
                qry = qry.Where( s => siteType.Contains( s.SiteType ) );
            }

            return qry.Take( RequestContext.GetPageParameter( "count" ).AsIntegerOrNull() ?? 1_000 );
        }

        /// <summary>
        /// Gets the grid builder that will be used to construct the definition
        /// and the final row data.
        /// </summary>
        /// <returns>A <see cref="GridBuilder{T}"/> instance that will handle building the grid data.</returns>
        protected override GridBuilder<Site> GetGridBuilder()
        {
            return new GridBuilder<Site>()
                .WithBlock( this )
                .AddField( "guid", p => p.Guid.ToString() )
                .AddField( "idKey", p => p.IdKey )
                .AddTextField( "name", p => p.Name )
                .AddTextField( "description", p => p.Description )
                .AddTextField( "siteIconUrl", p => GetSiteIconUrl( p ) )
                .AddTextField( "domains", p => p.SiteDomains.Select( a => a.Domain ).JoinStringsWithCommaAnd() )
                .AddTextField( "theme", p => p.Theme )
                .AddField( "isSystem", p => p.IsSystem )
                .AddAttributeFields( GetGridAttributes() );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new SiteService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                var sitePages = new List<int> {
                    entity.DefaultPageId ?? -1,
                    entity.LoginPageId ?? -1,
                    entity.RegistrationPageId ?? -1,
                    entity.PageNotFoundPageId ?? -1
                };

                var pageService = new PageService( rockContext );
                foreach ( var page in pageService.Queryable( "Layout" )
                    .Where( t => !t.IsSystem && ( t.Layout.SiteId == entity.Id || sitePages.Contains( t.Id ) ) ) )
                {
                    if ( pageService.CanDelete( page, out string deletePageErrorMessage ) )
                    {
                        pageService.Delete( page );
                    }
                }

                var layoutService = new LayoutService( rockContext );
                var layoutQry = layoutService.Queryable()
                    .Where( l =>
                    l.SiteId == entity.Id );
                layoutService.DeleteRange( layoutQry );

                rockContext.SaveChanges( true );

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out Site entity, out BlockActionResult error )
        {
            var entityService = new SiteService( rockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new Site();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Site.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Site.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the site icon.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <returns>A <see cref="string"/> site icon url.</returns>
        private string GetSiteIconUrl( Site site )
        {
            string path;

            // If this is a Person, use the Person properties.
            if ( site != null && site.FavIconBinaryFileId.HasValue )
            {
                path = string.Format( "/GetImage.ashx?id={0}&height=50px", site.FavIconBinaryFileId.Value );
            }
            // Otherwise, use the first letter of the entity type.
            else
            {
                path = $"/GetAvatar.ashx?text={site.Name.SubstringSafe( 0, 1 )}";
            }

            return path;
        }

        #endregion
    }
}
