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

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular note watch.
    /// </summary>

    [DisplayName( "Note Watch Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a note watch." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [EntityTypeField( "Entity Type",
        Description = "Set an Entity Type to limit this block to Note Types and Entities for a specific entity type.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.EntityType )]

    [NoteTypeField( "Note Type",
        Description = "Set Note Type to limit this block to a specific note type",
        AllowMultiple = false,
        Order = 1,
        Key = AttributeKey.NoteType )]

    #endregion

    [ContextAware( typeof( Rock.Model.Group ), typeof( Rock.Model.Person ) )]
    [Rock.SystemGuid.EntityTypeGuid( "02ee1267-4407-48f5-b28e-428de8297648" )]
    [Rock.SystemGuid.BlockTypeGuid( "b1f65833-ceca-4054-bcc3-2de5692741ed" )]
    public class NoteWatchDetail : RockDetailBlockType
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
            public const string EntityType = "EntityType";
            public const string NoteType = "NoteType";
            public const string WatchedNoteLavaTemplate = "WatchedNoteLavaTemplate";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<NoteWatchBag, NoteWatchDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<NoteWatch>();

                return box;
            }
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
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the NoteWatch is valid, <c>false</c> otherwise.</returns>
        private bool ValidateNoteWatch( NoteWatch noteWatch, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            // see if the Watcher parameters are valid
            if ( !noteWatch.IsValidWatcher )
            {
                errorMessage = "A Person or Group must be specified as the watcher";
                return false;
            }

            // see if the Watch filters parameters are valid
            if ( !noteWatch.IsValidWatchFilter )
            {
                errorMessage = "A Watch Filter must be specified.";
                return false;
            }

            if ( !noteWatch.IsValid )
            {
                errorMessage = noteWatch.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                return false;
            }

            // See if there is a matching filter that doesn't allow overrides
            if ( !noteWatch.IsWatching && !noteWatch.IsAbleToUnWatch( rockContext ) )
            {
                var nonOverridableNoteWatch = noteWatch.GetNonOverridableNoteWatches( rockContext ).FirstOrDefault();
                if ( nonOverridableNoteWatch != null )
                {
                    errorMessage = "Unable to set Watching to false. This would override another note watch that doesn't allow overrides.";
                    return false;
                }
            }

            // see if the NoteType allows following
            if ( noteWatch.NoteTypeId.HasValue )
            {
                var noteTypeCache = NoteTypeCache.Get( noteWatch.NoteTypeId.Value );
                if ( noteTypeCache != null && !noteTypeCache.AllowsWatching )
                {
                    errorMessage = "This note type doesn't allow note watches.";
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
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<NoteWatchBag, NoteWatchDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {NoteWatch.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            box.Options = GetBoxOptions( entity );

            entity.LoadAttributes( rockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
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
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( NoteWatch.FriendlyTypeName );
                }
            }
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

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="NoteWatchBag"/> that represents the entity.</returns>
        private NoteWatchBag GetEntityBagForView( NoteWatch entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            var watchedEntity = GetEntity( entity.EntityTypeId, entity.EntityId );
            bag.EntityName = watchedEntity?.ToString();

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="NoteWatchBag"/> that represents the entity.</returns>
        private NoteWatchBag GetEntityBagForEdit( NoteWatch entity )
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

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

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

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( NoteWatch entity, DetailBlockBox<NoteWatchBag, NoteWatchDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.AllowOverride ),
                () => entity.AllowOverride = box.Entity.AllowOverride );

            box.IfValidProperty( nameof( box.Entity.EntityType ),
                () => entity.EntityTypeId = box.Entity.EntityType.GetEntityId<EntityType>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.IsWatching ),
                () => entity.IsWatching = box.Entity.IsWatching );

            box.IfValidProperty( nameof( box.Entity.Note ),
                () => entity.NoteId = box.Entity.Note.GetEntityId<Note>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.NoteType ),
                () => entity.NoteTypeId = box.Entity.NoteType.GetEntityId<NoteType>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.WatcherGroup ),
                () => entity.WatcherGroupId = box.Entity.WatcherGroup.GetEntityId<Rock.Model.Group>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.WatcherPersonAlias ),
                () => entity.WatcherPersonAliasId = box.Entity.WatcherPersonAlias.GetEntityId<PersonAlias>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.EntityId ),
                () => entity.EntityId = GetEntityId( box.Entity, rockContext ) );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
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

            if ( bag.WatchedEntity == null )
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

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="NoteWatch"/> to be viewed or edited on the page.</returns>
        private NoteWatch GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<NoteWatch, NoteWatchService>( rockContext, PageParameterKey.NoteWatchId );
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
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                if ( entity != null )
                {
                    entity.LoadAttributes( rockContext );
                }

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( NoteWatch entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out NoteWatch entity, out BlockActionResult error )
        {
            var entityService = new NoteWatchService( rockContext );
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
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<NoteWatchBag, NoteWatchDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<NoteWatchBag, NoteWatchDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateNoteWatch( entity, rockContext, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                return ActionOk( this.GetParentPageUrl( GetQueryParams() ) );
            }
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
                var entityService = new NoteWatchService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

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
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<NoteWatchBag, NoteWatchDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<NoteWatchBag, NoteWatchDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                var oldAttributeGuids = box.Entity.Attributes.Values.Select( a => a.AttributeGuid ).ToList();
                var newAttributeGuids = refreshedBox.Entity.Attributes.Values.Select( a => a.AttributeGuid );

                // If the attributes haven't changed then return a 204 status code.
                if ( oldAttributeGuids.SequenceEqual( newAttributeGuids ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
                }

                // Replace any values for attributes that haven't changed with
                // the value sent by the client. This ensures any unsaved attribute
                // value changes are not lost.
                foreach ( var kvp in refreshedBox.Entity.Attributes )
                {
                    if ( oldAttributeGuids.Contains( kvp.Value.AttributeGuid ) )
                    {
                        refreshedBox.Entity.AttributeValues[kvp.Key] = box.Entity.AttributeValues[kvp.Key];
                    }
                }

                return ActionOk( refreshedBox );
            }
        }

        /// <summary>
        /// Gets an Entity's name by type and entity Id.
        /// </summary>
        /// <param name="entityId">The Entity Id</param>
        /// <param name="entityTypeId">The EntityType Id</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetEntityName( int? entityId, Guid? entityTypeGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                string entityName = string.Empty;

                if ( entityId.HasValue && entityTypeGuid.HasValue )
                {
                    var entityType = EntityTypeCache.Get( entityTypeGuid.Value );
                    var watchedEntity = new EntityTypeService( rockContext ).GetEntity( entityType.Id, entityId.Value );
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
        }

        /// <summary>
        /// Get the available note type options based on the selected entity Id
        /// </summary>
        /// <param name="entityTypeId">The EntityType Id</param>
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
