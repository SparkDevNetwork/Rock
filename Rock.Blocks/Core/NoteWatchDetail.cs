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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.NoteWatchDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular note watch.
    /// </summary>

    [DisplayName( "Note Watch Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a note watch." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CodeEditorField( "Watched Note Lava Template",
        Key = AttributeKey.WatchedNoteLavaTemplate,
        Description = "The Lava template to use to show the watched note type. <span class='tip tip-lava'></span>",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 400,
        IsRequired = false )]

    #endregion

    [ContextAware( typeof( Rock.Model.Group ), typeof( Rock.Model.Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "02ee1267-4407-48f5-b28e-428de8297648" )]
    [Rock.SystemGuid.BlockTypeGuid( "b1f65833-ceca-4054-bcc3-2de5692741ed" )]
    public class NoteWatchDetail : RockEntityDetailBlockType<NoteWatch, NoteWatchBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string NoteWatchId = "NoteWatchId";
            public const string GroupId = "GroupId";
            public const string PersonId = "PersonId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        public static class AttributeKey
        {
            public const string WatchedNoteLavaTemplate = "WatchedNoteLavaTemplate";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<NoteWatchBag, NoteWatchDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private NoteWatchDetailOptionsBag GetBoxOptions( NoteWatch entity )
        {
            var isNew = entity.Id == 0;

            var options = new NoteWatchDetailOptionsBag();

            options.NoteTypeOptions = GetNoteTypeOptions( entity.EntityTypeId );

            var contextPerson = RequestContext.GetContextEntity<Person>();
            var contextGroup = RequestContext.GetContextEntity<Rock.Model.Group>();

            if ( isNew )
            {
                if ( contextPerson != null )
                {
                    entity.WatcherPersonAliasId = contextPerson.PrimaryAliasId;
                    entity.WatcherPersonAlias = contextPerson.PrimaryAlias;
                }
                else if ( contextGroup != null )
                {
                    entity.WatcherGroupId = contextGroup.Id;
                    entity.WatcherGroup = contextGroup;
                }
            }

            if ( contextPerson != null )
            {
                options.DisablePersonPicker = true;
                options.HideGroupPicker = true;

                // make sure we are seeing details for a NoteWatch that the current person is watching
                if ( !entity.WatcherPersonAliasId.HasValue || !contextPerson.Aliases.Any( a => a.Id == entity.WatcherPersonAliasId.Value ) )
                {
                    // The NoteWatchId in the url isn't a NoteWatch for the PersonContext, so just hide the block
                    options.HidePanel = true;
                }
            }
            else if ( contextGroup != null )
            {
                options.HidePersonPicker = true;
                options.DisableGroupPicker = true;

                // make sure we are seeing details for a NoteWatch that the current group context is watching
                if ( !entity.WatcherGroupId.HasValue || ( contextGroup.Id != entity.WatcherGroupId ) )
                {
                    // The NoteWatchId in the url isn't a NoteWatch for the GroupContext, so just hide the block
                    options.HidePanel = true;
                }
            }

            return options;
        }

        /// <summary>
        /// Validates the NoteWatch for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="noteWatch">The NoteWatch to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the NoteWatch is valid, <c>false</c> otherwise.</returns>
        private bool ValidateNoteWatch( NoteWatch noteWatch, out string errorMessage )
        {
            errorMessage = null;

            // see if the Watcher parameters are valid
            if ( !noteWatch.IsValidWatcher )
            {
                errorMessage = "WatcherMustBeSelected";
                return false;
            }

            // see if the Watch filters parameters are valid
            if ( !noteWatch.IsValidWatchFilter )
            {
                errorMessage = "WatchFilterMustBeSelected";
                return false;
            }

            if ( !noteWatch.IsValid )
            {
                errorMessage = noteWatch.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                return false;
            }

            // See if there is a matching filter that doesn't allow overrides
            if ( !noteWatch.IsWatching && !noteWatch.IsAbleToUnWatch( RockContext ) )
            {
                var nonOverridableNoteWatch = noteWatch.GetNonOverridableNoteWatches( RockContext ).FirstOrDefault();
                if ( nonOverridableNoteWatch != null )
                {
                    errorMessage = "UnableToOverride";
                    return false;
                }
            }

            // see if the NoteType allows following
            if ( noteWatch.NoteTypeId.HasValue )
            {
                var noteTypeCache = NoteTypeCache.Get( noteWatch.NoteTypeId.Value );
                if ( noteTypeCache != null && !noteTypeCache.AllowsWatching )
                {
                    errorMessage = "NoteTypeDoesNotAllowWatch";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<NoteWatchBag, NoteWatchDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {NoteWatch.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            box.Options = GetBoxOptions( entity );

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( NoteWatch.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( NoteWatch.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="NoteWatchBag"/> that represents the entity.</returns>
        private NoteWatchBag GetCommonEntityBag( NoteWatch entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new NoteWatchBag
            {
                IdKey = entity.IdKey,
                AllowOverride = entity.AllowOverride,
                EntityType = entity.EntityType.ToListItemBag(),
                IsWatching = entity.IsWatching,
                Note = entity.Note.ToListItemBag(),
                NoteType = entity.NoteType.ToListItemBag(),
                WatcherGroup = entity.WatcherGroup.ToListItemBag(),
                WatcherPersonAlias = entity.WatcherPersonAlias.ToListItemBag(),
            };
        }

        /// <inheritdoc/>
        protected override NoteWatchBag GetEntityBagForView( NoteWatch entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            var watchedEntity = GetEntity( entity.EntityTypeId, entity.EntityId );
            bag.EntityName = watchedEntity?.ToString();

            return bag;
        }

        /// <inheritdoc/>
        protected override NoteWatchBag GetEntityBagForEdit( NoteWatch entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            var watchedEntity = GetEntity( entity.EntityTypeId, entity.EntityId );
            if ( watchedEntity is Person person )
            {
                bag.WatchedEntity = person.PrimaryAlias.ToListItemBag();
            }
            else if ( watchedEntity is Rock.Model.Group )
            {
                bag.WatchedEntity = watchedEntity.ToListItemBag();
            }
            else
            {
                bag.EntityId = entity.EntityId;
                bag.EntityName = watchedEntity?.ToString();
            }

            if ( entity.Note != null )
            {
                var mergefields = RequestContext.GetCommonMergeFields();
                mergefields.Add( "Note", entity.Note );
                var lavaTemplate = this.GetAttributeValue( AttributeKey.WatchedNoteLavaTemplate );

                bag.WatchedNoteText = lavaTemplate.ResolveMergeFields( mergefields );
            }

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <summary>
        /// Gets an Entity by type and entity Id.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <returns></returns>
        private IEntity GetEntity( int? entityTypeId, int? entityId )
        {
            if ( !entityTypeId.HasValue || !entityId.HasValue )
            {
                return null;
            }

            return new EntityTypeService( new RockContext() ).GetEntity( entityTypeId.Value, entityId.Value );
        }

        /// <summary>
        /// Get the available note type options based on the selected entity Id
        /// </summary>
        /// <param name="entityTypeId"></param>
        /// <returns></returns>
        private List<ListItemBag> GetNoteTypeOptions( int? entityTypeId )
        {
            if ( entityTypeId.HasValue )
            {
                var entityNoteTypes = NoteTypeCache.GetByEntity( entityTypeId.Value, null, null, true );
                return entityNoteTypes.ToListItemBagList();
            }

            return new List<ListItemBag>();
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( NoteWatch entity, ValidPropertiesBox<NoteWatchBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.AllowOverride ),
                () => entity.AllowOverride = box.Bag.AllowOverride );

            box.IfValidProperty( nameof( box.Bag.EntityType ),
                () => entity.EntityTypeId = box.Bag.EntityType.GetEntityId<EntityType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.IsWatching ),
                () => entity.IsWatching = box.Bag.IsWatching );

            box.IfValidProperty( nameof( box.Bag.Note ),
                () => entity.NoteId = box.Bag.Note.GetEntityId<Note>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.NoteType ),
                () => entity.NoteTypeId = box.Bag.NoteType.GetEntityId<NoteType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.WatcherGroup ),
                () => entity.WatcherGroupId = box.Bag.WatcherGroup.GetEntityId<Rock.Model.Group>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.WatcherPersonAlias ),
                () => entity.WatcherPersonAliasId = box.Bag.WatcherPersonAlias.GetEntityId<PersonAlias>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.EntityId ),
                () => entity.EntityId = GetEntityId( box.Bag, RockContext ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            return true;
        }

        /// <summary>
        /// Gets the selected watched entity id.
        /// </summary>
        /// <param name="bag"></param>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private int? GetEntityId( NoteWatchBag bag, RockContext rockContext )
        {
            int? entityId = null;

            if ( string.IsNullOrWhiteSpace( bag.WatchedEntity?.Value ) )
            {
                entityId = bag.EntityId;
            }
            else if ( bag.EntityType?.Value?.ToUpper() == Rock.SystemGuid.EntityType.GROUP )
            {
                entityId = bag.WatchedEntity.GetEntityId<Rock.Model.Group>( rockContext );
            }
            else if ( bag.EntityType?.Value?.ToUpper() == Rock.SystemGuid.EntityType.PERSON )
            {
                var personAliasService = new PersonAliasService( rockContext );
                var personAlias = personAliasService.Get( bag.WatchedEntity.Value.AsGuid() );
                entityId = personAlias?.PersonId;
            }

            return entityId;
        }

        /// <inheritdoc/>
        protected override NoteWatch GetInitialEntity()
        {
            return GetInitialEntity<NoteWatch, NoteWatchService>( RockContext, PageParameterKey.NoteWatchId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var queryParams = GetQueryParams();

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( queryParams )
            };
        }

        /// <summary>
        /// Gets the query parameters for constructing Urls.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetQueryParams()
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();

            var contextPerson = RequestContext.GetContextEntity<Person>();
            var contextGroup = RequestContext.GetContextEntity<Rock.Model.Group>();

            if ( contextPerson != null )
            {
                queryParams.Add( PageParameterKey.PersonId, contextPerson.Id.ToString() );
            }

            if ( contextGroup != null )
            {
                queryParams.Add( PageParameterKey.GroupId, contextGroup.Id.ToString() );
            }

            return queryParams;
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out NoteWatch entity, out BlockActionResult error )
        {
            var entityService = new NoteWatchService( RockContext );
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
                entity = new NoteWatch();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{NoteWatch.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${NoteWatch.FriendlyTypeName}." );
                return false;
            }

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

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<NoteWatchBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<NoteWatchBag> box )
        {
            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateNoteWatch( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            return ActionOk( this.GetParentPageUrl( GetQueryParams() ) );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new NoteWatchService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        /// <summary>
        /// Gets an Entity's name by type and entity Id.
        /// </summary>
        /// <param name="entityId">The Entity Id</param>
        /// <param name="entityTypeGuid">The EntityType guid</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetEntityName( int? entityId, Guid? entityTypeGuid )
        {
            string entityName = string.Empty;

            if ( entityId.HasValue && entityTypeGuid.HasValue )
            {
                var entityType = EntityTypeCache.Get( entityTypeGuid.Value );
                var watchedEntity = new EntityTypeService( RockContext ).GetEntity( entityType.Id, entityId.Value );
                if ( watchedEntity != null )
                {
                    entityName = watchedEntity.ToString();
                }
                else
                {
                    entityName = string.Format( "<span class='label label-danger'>{0} with Id {1} not found</span>", EntityTypeCache.Get( entityTypeGuid.Value ).FriendlyName, entityId );
                }
            }

            return ActionOk( new { entityName = entityName } );
        }

        /// <summary>
        /// Get the available note type options based on the selected entity Id
        /// </summary>
        /// <param name="entityTypeGuid">The EntityType guid</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetNoteTypes( Guid? entityTypeGuid )
        {
            List<ListItemBag> options;

            if ( entityTypeGuid.HasValue )
            {
                var entityType = EntityTypeCache.Get( entityTypeGuid.Value );
                options = GetNoteTypeOptions( entityType.Id );
            }
            else
            {
                options = new List<ListItemBag>();
            }

            return ActionOk( new { noteTypeOptions = options } );
        }

        #endregion
    }
}
