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
using System.Data.Entity;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Reminders.ReminderTypeList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Reminders
{
    /// <summary>
    /// Displays a list of reminder types with inline modal editing.
    /// </summary>

    [DisplayName( "Reminder Types" )]
    [Category( "Reminders" )]
    [Description( "Displays a list of reminder types." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( SiteType.Web )]

    [SystemGuid.EntityTypeGuid( "ADD3CE44-15C5-4E96-A9AA-7B31E20ECB3E" )]
    // was SystemGuid.BlockTypeGuid( "DC87912A-D9A5-43A1-9746-74CAF5E5DA48" )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.REMINDER_TYPES )]
    [CustomizedGrid]
    public class ReminderTypeList : RockEntityListBlockType<ReminderType>
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<ReminderTypeListOptionsBag>();
            var builder = GetGridBuilder();

            var isAddDeleteEnabled = GetIsAddDeleteEnabled();
            box.IsAddEnabled = isAddDeleteEnabled;
            box.IsDeleteEnabled = isAddDeleteEnabled;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private ReminderTypeListOptionsBag GetBoxOptions()
        {
            return new ReminderTypeListOptionsBag();
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// </summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddDeleteEnabled()
        {
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <inheritdoc/>
        protected override IQueryable<ReminderType> GetListQueryable( RockContext rockContext )
        {
            return base.GetListQueryable( rockContext )
                .Include( rt => rt.EntityType );
        }

        /// <inheritdoc/>
        protected override IQueryable<ReminderType> GetOrderedListQueryable( IQueryable<ReminderType> queryable, RockContext rockContext )
        {
            return queryable
                .OrderBy( rt => rt.Order )
                .ThenBy( rt => rt.Name )
                .ThenBy( rt => rt.Id );
        }

        /// <inheritdoc/>
        protected override List<ReminderType> GetListItems( IQueryable<ReminderType> queryable, RockContext rockContext )
        {
            return queryable.ToList()
                .Where( rt => rt.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();
        }

        /// <inheritdoc/>
        protected override GridBuilder<ReminderType> GetGridBuilder()
        {
            return new GridBuilder<ReminderType>()
                .WithBlock( this )
                .AddTextField( "guid", a => a.Guid.ToString() )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "entityType", a => a.EntityType != null ? a.EntityType.FriendlyName : string.Empty )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
        }

        /// <summary>
        /// Gets the entity bag that is used when entering edit mode.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="ReminderTypeBag"/> that represents the entity.</returns>
        private ReminderTypeBag GetEntityBagForEdit( ReminderType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            // Display Person instead of PersonAlias to the user since
            // PersonAlias is an internal detail the user shouldn't see.
            var entityTypeId = entity.EntityTypeId;
            if ( entityTypeId == EntityTypeCache.GetId<PersonAlias>() )
            {
                entityTypeId = EntityTypeCache.GetId<Person>().Value;
            }

            var entityTypeCache = EntityTypeCache.Get( entityTypeId );

            return new ReminderTypeBag
            {
                Guid = entity.Guid.ToString(),
                Name = entity.Name,
                Description = entity.Description,
                IsActive = entity.IsActive,
                NotificationType = entity.NotificationType,
                NotificationWorkflowType = entity.NotificationWorkflowTypeId.HasValue
                    ? WorkflowTypeCache.Get( entity.NotificationWorkflowTypeId.Value )?.ToListItemBag()
                    : null,
                ShouldShowNote = entity.ShouldShowNote,
                EntityType = entityTypeCache.ToListItemBag(),
                ShouldAutoCompleteWhenNotified = entity.ShouldAutoCompleteWhenNotified,
                HighlightColor = entity.HighlightColor
            };
        }

        /// <summary>
        /// Tries to get the entity for the edit action, either by loading
        /// an existing one or creating a new one.
        /// </summary>
        /// <param name="guid">The Guid of the reminder type to load, or empty to create a new one.</param>
        /// <param name="entity">On return of <c>true</c> contains the reminder type that was loaded or created.</param>
        /// <param name="errorMessage">On return of <c>false</c> contains the error result to respond with.</param>
        /// <returns><c>true</c> if an entity was loaded or created; otherwise <c>false</c>.</returns>
        private bool TryGetEntityForEditAction( string guid, out ReminderType entity, out BlockActionResult errorMessage )
        {
            var entityService = new ReminderTypeService( RockContext );
            errorMessage = null;

            // Determine if we are editing an existing entity or creating a new one.
            var entityGuid = guid.AsGuidOrNull();
            if ( entityGuid.HasValue )
            {
                entity = entityService.Get( entityGuid.Value );
            }
            else
            {
                entity = new ReminderType
                {
                    IsActive = true,
                    ShouldShowNote = true
                };

                entityService.Add( entity );
            }

            if ( entity == null )
            {
                errorMessage = ActionBadRequest( $"{ReminderType.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                errorMessage = ActionBadRequest( $"Not authorized to edit {ReminderType.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates the entity from the information contained in the box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box that contains the data.</param>
        /// <returns><c>true</c> if the update was successful; otherwise <c>false</c>.</returns>
        private bool UpdateEntityFromBox( ReminderType entity, ValidPropertiesBox<ReminderTypeBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.NotificationType ),
                () => entity.NotificationType = box.Bag.NotificationType ?? ReminderNotificationType.Communication );

            box.IfValidProperty( nameof( box.Bag.NotificationWorkflowType ),
                () => entity.NotificationWorkflowTypeId = box.Bag.NotificationWorkflowType?.Value?.AsGuidOrNull() != null
                    ? WorkflowTypeCache.Get( box.Bag.NotificationWorkflowType.Value.AsGuid() )?.Id
                    : null );

            box.IfValidProperty( nameof( box.Bag.ShouldShowNote ),
                () => entity.ShouldShowNote = box.Bag.ShouldShowNote );

            box.IfValidProperty( nameof( box.Bag.EntityType ),
                () =>
                {
                    var entityTypeGuid = box.Bag.EntityType?.Value?.AsGuidOrNull();
                    if ( entityTypeGuid.HasValue )
                    {
                        var entityTypeId = EntityTypeCache.GetId( entityTypeGuid.Value );

                        // Store PersonAlias instead of Person since reminders
                        // internally reference PersonAlias records.
                        if ( entityTypeId == EntityTypeCache.GetId<Person>() )
                        {
                            entityTypeId = EntityTypeCache.GetId<PersonAlias>();
                        }

                        entity.EntityTypeId = entityTypeId ?? 0;
                    }
                } );

            box.IfValidProperty( nameof( box.Bag.ShouldAutoCompleteWhenNotified ),
                () => entity.ShouldAutoCompleteWhenNotified = box.Bag.ShouldAutoCompleteWhenNotified );

            box.IfValidProperty( nameof( box.Bag.HighlightColor ),
                () => entity.HighlightColor = box.Bag.HighlightColor );

            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<ReminderTypeBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>The updated entity bag after saving.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<ReminderTypeBag> box )
        {
            var entityService = new ReminderTypeService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.Guid, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // If this is a new entity, set the order to the end of the list.
            var isNew = entity.Id == 0;
            if ( isNew )
            {
                var orders = entityService.Queryable()
                    .Select( d => d.Order )
                    .ToList();

                entity.Order = orders.Any() ? orders.Max() + 1 : 0;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new ReminderTypeService( RockContext );
            var entityGuid = key.AsGuidOrNull();
            var entity = entityGuid.HasValue ? entityService.Get( entityGuid.Value ) : null;

            if ( entity == null )
            {
                return ActionBadRequest( $"{ReminderType.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {ReminderType.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Reorders the items in the list.
        /// </summary>
        /// <param name="key">The identifier of the item being moved.</param>
        /// <param name="beforeKey">The identifier of the item it should be placed before, or an empty string to move to the end.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderItem( string key, string beforeKey )
        {
            var qry = GetListQueryable( RockContext );
            qry = GetOrderedListQueryable( qry, RockContext );
            var items = GetListItems( qry, RockContext );

            if ( !items.ReorderEntity( key, beforeKey ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();
            return ActionOk();
        }

        #endregion
    }
}
