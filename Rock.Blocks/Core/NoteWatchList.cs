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

    [EntityTypeField( "Entity Type",
        Description = "Set an Entity Type to limit this block to Note Types and Entities for a specific entity type.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.EntityType )]

    [NoteTypeField( "Note Type",
        Description = "Set Note Type to limit this block to a specific note type",
        AllowMultiple = false,
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.NoteType )]

    [Rock.SystemGuid.EntityTypeGuid( "8fdb4340-bdde-4797-b173-ea456a825b2a" )]
    [Rock.SystemGuid.BlockTypeGuid( "ed4cd6ae-ed86-4607-a252-f15971e4f2e3" )]
    [CustomizedGrid]
    public class NoteWatchList : RockEntityListBlockType<NoteWatch>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string EntityType = "EntityType";
            public const string NoteType = "NoteType";
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
            options.EntityTypeGuid = GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull();
            options.NoteTypeGuid = GetAttributeValue( AttributeKey.NoteType ).AsGuidOrNull();
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
            var qry = base.GetListQueryable( rockContext )
                .Include( a => a.WatcherPersonAlias )
                .Include( a => a.WatcherGroup )
                .Include( a => a.NoteType )
                .Include( a => a.EntityType );

            Guid? blockEntityTypeGuid = GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull();
            Guid? blockNoteTypeGuid = GetAttributeValue( AttributeKey.NoteType ).AsGuidOrNull();

            if ( blockNoteTypeGuid.HasValue )
            {
                var noteType = NoteTypeCache.Get( blockNoteTypeGuid.Value );
                if ( noteType != null )
                {
                    int noteTypeId = noteType.Id;
                    qry = qry.Where( a => a.NoteTypeId.HasValue && a.NoteTypeId == noteTypeId );
                }
            }
            else if ( blockEntityTypeGuid.HasValue )
            {
                var entityType = EntityTypeCache.Get( blockEntityTypeGuid.Value );
                if ( entityType != null )
                {
                    int entityTypeId = entityType.Id;
                    qry = qry.Where( a =>
                        ( a.EntityTypeId.HasValue && a.EntityTypeId.Value == entityTypeId ) ||
                        ( a.NoteTypeId.HasValue && a.NoteType.EntityTypeId == entityTypeId ) );
                }
            }

            var contextEntity = GetContextEntity();
            if ( contextEntity is Person contextPerson )
            {
                qry = qry.Where( a => a.WatcherPersonAliasId.HasValue && a.WatcherPersonAlias.PersonId == contextPerson.Id );
            }
            else if ( contextEntity is Model.Group contextGroup )
            {
                qry = qry.Where( a => a.WatcherGroupId.HasValue && a.WatcherGroupId == contextGroup.Id );
            }

            // Add null check for the data source
            if ( qry != null )
            {
                return qry;
            }
            else
            {
                return Enumerable.Empty<NoteWatch>().AsQueryable();
            }
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
            var entityService = new NoteWatchService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{NoteWatch.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {NoteWatch.FriendlyTypeName}." );
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
