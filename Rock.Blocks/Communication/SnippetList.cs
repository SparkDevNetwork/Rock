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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Communication.SnippetList;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Displays a list of snippets filtered by the configured snippet type.
    /// </summary>
    
    [DisplayName( "Snippet List" )]
    [Category( "Communication" )]
    [Description( "Lists the snippets currently in the system." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Snippet Detail",
        Description = "The page that will show the snippet details.",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [CustomDropdownListField( "Snippet Type",
        Description = "Determines what type of snippet to filter on. This is required (only one type can be displayed at a time).",
        ListSource = "SELECT [Guid] as [Value], [Name] as [Text] FROM [SnippetType]",
        IsRequired = true,
        Key = AttributeKey.SnippetType,
        Order = 1 )]

    [BooleanField( "Show Personal Column",
        Description = "Determines if the personal column is displayed. Not all types will support this.",
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowPersonalColumn,
        Order = 2 )]

    [Rock.SystemGuid.EntityTypeGuid( "277D2E99-F7A6-4256-BE61-1539624277AD" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "3635AD67-01CF-4F85-9D1F-18A7B2D0DBBB" )]
    [Rock.SystemGuid.BlockTypeGuid( "2EDAD934-6129-480B-9812-4BA7B9978AD2" )]
    [CustomizedGrid]
    public class SnippetList : RockEntityListBlockType<Snippet>
    {
        #region Keys

        /// <summary>
        /// Keys to use for block attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "SnippetDetail";
            public const string SnippetType = "SnippetType";
            public const string ShowPersonalColumn = "ShowPersonalColumn";
        }

        /// <summary>
        /// Keys to use for navigation URLs.
        /// </summary>
        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<SnippetListOptionsBag>();
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
        private SnippetListOptionsBag GetBoxOptions()
        {
            var options = new SnippetListOptionsBag
            {
                IsShowPersonalColumnEnabled = GetAttributeValue( AttributeKey.ShowPersonalColumn ).AsBoolean()
            };

            // Build a dynamic title from the configured snippet type name.
            var snippetTypeGuid = GetAttributeValue( AttributeKey.SnippetType ).AsGuidOrNull();
            if ( snippetTypeGuid.HasValue )
            {
                var snippetTypeName = new SnippetTypeService( RockContext )
                    .GetSelect( snippetTypeGuid.Value, s => s.Name );

                if ( snippetTypeName.IsNotNullOrWhiteSpace() )
                {
                    options.Title = $"{snippetTypeName} Snippets";
                }
            }

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// </summary>
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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "SnippetId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Snippet> GetListQueryable( RockContext rockContext )
        {
            var queryable = base.GetListQueryable( rockContext );

            // Filter to only the configured snippet type.
            var snippetTypeGuid = GetAttributeValue( AttributeKey.SnippetType ).AsGuidOrNull();
            if ( snippetTypeGuid.HasValue )
            {
                var snippetTypeId = new SnippetTypeService( rockContext )
                    .GetId( snippetTypeGuid.Value );

                if ( snippetTypeId.HasValue )
                {
                    queryable = queryable.Where( s => s.SnippetTypeId == snippetTypeId.Value );
                }
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<Snippet> GetOrderedListQueryable( IQueryable<Snippet> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( s => s.Name ).ThenBy( s => s.Id );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Snippet> GetGridBuilder()
        {
            return new GridBuilder<Snippet>()
                .WithBlock( this )
                .AddTextField( "idKey", s => s.IdKey )
                .AddTextField( "name", s => s.Name )
                .AddTextField( "description", s => s.Description )
                .AddField( "isPersonal", s => s.OwnerPersonAliasId.HasValue )
                .AddField( "isActive", s => s.IsActive )
                .AddField( "isSecurityDisabled", s => !s.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
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
            var entityService = new SnippetService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{Snippet.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {Snippet.FriendlyTypeName}." );
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
