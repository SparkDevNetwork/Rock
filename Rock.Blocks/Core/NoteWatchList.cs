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
using Rock.ViewModels.Blocks.Core.NoteWatchList;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of note watches.
    /// </summary>

    [DisplayName( "Note Watch List" )]
    [Category( "Core" )]
    [Description( "Displays a list of note watches." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the note watch details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "8fdb4340-bdde-4797-b173-ea456a825b2a" )]
    [Rock.SystemGuid.BlockTypeGuid( "ed4cd6ae-ed86-4607-a252-f15971e4f2e3" )]
    [CustomizedGrid]
    public class NoteWatchList : RockEntityListBlockType<NoteWatch>
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

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<NoteWatchListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEnabled();
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
        private NoteWatchListOptionsBag GetBoxOptions()
        {
            var options = new NoteWatchListOptionsBag();

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new NoteWatch();

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "NoteWatchId", "((Key))" )
            };
        }

        private string FormatEntityType( EntityType entityType, int? entityId )
        {
            var entityTypeName = entityType?.FriendlyName ?? "Unknown";
            if ( entityId.HasValue && entityType != null )
            {
                var rockContext = new RockContext();
                var entity = new EntityTypeService( rockContext ).GetEntity( entityType.Id, entityId.Value );
                if ( entity != null )
                {
                    var entityName = entity.ToString(); 
                    return $"{entityTypeName} ({entityName})";
                }
            }

            return entityTypeName;
        }

        /// <inheritdoc/>
        protected override IQueryable<NoteWatch> GetListQueryable( RockContext rockContext )
        {
            return base.GetListQueryable( rockContext )
                .Include( a => a.WatcherPersonAlias )
                .Include( a => a.WatcherGroup )
                .Include( a => a.NoteType )
                .Include( a => a.EntityType );
        }

        /// <inheritdoc/>
        protected override GridBuilder<NoteWatch> GetGridBuilder()
        {
            return new GridBuilder<NoteWatch>()
                .WithBlock( this )
                .AddField( "id", a => a.Id )
                .AddTextField( "idKey", a => a.IdKey )
                .AddField( "isWatching", a => a.IsWatching )
                .AddPersonField( "watcher", a => a.WatcherPersonAlias?.Person )
                .AddTextField( "watcherGroup", a => a.WatcherGroup?.Name )
                .AddTextField( "noteType", a => a.NoteType?.Name )
                .AddTextField("entityType", a => FormatEntityType(a.EntityType, a.EntityId))
                .AddField( "allowOverride", a => a.AllowOverride )
                .AddAttributeFields( GetGridAttributes() );
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
                var entityService = new NoteWatchService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{NoteWatch.FriendlyTypeName} not found." );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete ${NoteWatch.FriendlyTypeName}." );
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

        #endregion
    }
}
