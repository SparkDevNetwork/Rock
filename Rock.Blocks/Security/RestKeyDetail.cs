﻿// <copyright>
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
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Security.RestKeyDetail;
using Rock.Web.Cache;

namespace Rock.Blocks.Security
{
    /// <summary>
    /// Displays the details of a particular user login.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Rest Key Detail" )]
    [Category( "Security" )]
    [Description( "Displays the details of a particular user login." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "aed330ca-40a4-407a-b2dc-a0c1310fdc39" )]
    [Rock.SystemGuid.BlockTypeGuid( "28A34F1C-80F4-496F-A598-180974ADEE61" )]
    public class RestKeyDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string RestUserId = "RestUserId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<RestKeyBag, RestKeyDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private RestKeyDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new RestKeyDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the UserLogin for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="userLogin">The UserLogin to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the UserLogin is valid, <c>false</c> otherwise.</returns>
        private bool ValidateUserLogin( UserLogin userLogin, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            var userLoginInDb = new UserLoginService(rockContext).Queryable().Where( a => a.ApiKey == userLogin.ApiKey ).FirstOrDefault();
            if ( userLoginInDb != null && userLoginInDb.PersonId != userLogin.PersonId )
            {
                // this key already exists in the database. Show the error and get out of here.
                errorMessage = "This API Key already exists. Please enter a different one, or generate one by clicking the 'Generate Key' button below. ";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<RestKeyBag, RestKeyDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {UserLogin.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity, rockContext );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( UserLogin.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity, rockContext );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( UserLogin.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <param name="rockContext">The rock Context.</param>
        /// <returns>A <see cref="RestKeyBag"/> that represents the entity.</returns>
        private RestKeyBag GetCommonEntityBag( UserLogin entity, RockContext rockContext )
        {
            if ( entity == null || rockContext == null )
            {
                return null;
            }

            var restKeyBag = new RestKeyBag
            {
                IdKey = entity.IdKey,
                ApiKey = entity.ApiKey,
                EntityType = entity.EntityType.ToListItemBag()
            };

            if ( entity.Person == null )
            {
                restKeyBag.IsActive = true;
            }
            else
            {
                restKeyBag.Name = entity.Person.LastName;
                restKeyBag.IsActive = entity.Person.RecordStatusValueId == DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
                var noteService = new NoteService( rockContext );
                // the description gets saved as a system note for the person
                var noteType = NoteTypeCache.Get( Rock.SystemGuid.NoteType.PERSON_TIMELINE_NOTE.AsGuid() );
                var description = noteService.Queryable().Where( a => a.EntityId == entity.Person.Id && a.NoteTypeId == noteType.Id ).FirstOrDefault();
                if ( description != null )
                {
                    restKeyBag.Description = description.Text;
                }

            }

            return restKeyBag;
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <param name="rockContext">The rock Context.</param>
        /// <returns>A <see cref="RestKeyBag"/> that represents the entity.</returns>
        private RestKeyBag GetEntityBagForView( UserLogin entity, RockContext rockContext )
        {
            if ( entity == null || rockContext == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity, rockContext );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <param name="rockContext">The rock Context.</param>
        /// <returns>A <see cref="RestKeyBag"/> that represents the entity.</returns>
        private RestKeyBag GetEntityBagForEdit( UserLogin entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity, rockContext );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( UserLogin entity, DetailBlockBox<RestKeyBag, RestKeyDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.ApiKey ),
                () => entity.ApiKey = box.Entity.ApiKey );

            box.IfValidProperty( nameof( box.Entity.EntityType ),
                () => entity.EntityTypeId = box.Entity.EntityType.GetEntityId<EntityType>( rockContext ) );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="UserLogin"/> to be viewed or edited on the page.</returns>
        private UserLogin GetInitialEntity( RockContext rockContext )
        {
            var person = GetInitialEntity<Person, PersonService>( rockContext, PageParameterKey.RestUserId );
            if ( person != null)
            {
                if ( person.RecordTypeValueId == DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_RESTUSER.AsGuid() ).Id && person.Users.Any( a => a.ApiKey.IsNotNullOrWhiteSpace() ) )
                {
                    return person.Users.Where( a => a.ApiKey.IsNotNullOrWhiteSpace() ).FirstOrDefault();
                }
                else
                {
                    return new UserLogin
                    {
                        Id = 0,
                        Guid = Guid.Empty
                    };
                }
            }

            return null;            
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( UserLogin entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out UserLogin entity, out BlockActionResult error )
        {
            var entityService = new UserLoginService( rockContext );
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
                entity = new UserLogin();
                entity.Person = new Person();
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{UserLogin.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${UserLogin.FriendlyTypeName}." );
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

                var box = new DetailBlockBox<RestKeyBag, RestKeyDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
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
        public BlockActionResult Save( DetailBlockBox<RestKeyBag, RestKeyDetailOptionsBag> box )
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
                if ( !ValidateUserLogin( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                var entityService = new UserLoginService( rockContext );
                var restUser = new Person();
                rockContext.WrapTransaction( () =>
                {
                    var personService = new PersonService( rockContext );
                    
                    if ( entity.PersonId.HasValue && entity.PersonId.Value != 0 )
                    {
                        restUser = personService.Get( entity.PersonId.Value );
                    }
                    else
                    {
                        personService.Add( restUser );
                        rockContext.SaveChanges();
                    }

                    // the rest user name gets saved as the last name on a person
                    box.IfValidProperty( nameof( box.Entity.Name ),
                        () => restUser.LastName = box.Entity.Name );

                    box.IfValidProperty( nameof( box.Entity.IsActive ),
                        () => restUser.RecordStatusValueId = DefinedValueCache.Get( box.Entity.IsActive ? Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() : Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id );

                    restUser.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_RESTUSER.AsGuid() ).Id;
                    if ( restUser.IsValid )
                    {
                        rockContext.SaveChanges();
                    }

                    // the description gets saved as a system note for the person
                    var noteType = NoteTypeCache.Get( Rock.SystemGuid.NoteType.PERSON_TIMELINE_NOTE.AsGuid() );
                    if ( noteType != null )
                    {
                        var noteService = new NoteService( rockContext );
                        var note = noteService.Get( noteType.Id, restUser.Id ).FirstOrDefault();
                        if ( note == null )
                        {
                            note = new Note();
                            noteService.Add( note );
                        }
                        note.NoteTypeId = noteType.Id;
                        note.EntityId = restUser.Id;
                        note.Text = box.Entity.Description;
                    }

                    rockContext.SaveChanges();

                    // the key gets saved in the api key field of a user login (which you have to create if needed)
                    var entityType = new EntityTypeService( rockContext )
                        .Get( "Rock.Security.Authentication.Database" );
                    var userLogin = entityService.GetByPersonId( restUser.Id ).FirstOrDefault();
                    if ( userLogin == null )
                    {
                        userLogin = new UserLogin();
                        entityService.Add( userLogin );
                    }

                    if ( string.IsNullOrWhiteSpace( userLogin.UserName ) )
                    {
                        userLogin.UserName = Guid.NewGuid().ToString();
                    }

                    userLogin.IsConfirmed = true;
                    userLogin.ApiKey = entity.ApiKey;
                    userLogin.PersonId = restUser.Id;
                    userLogin.EntityTypeId = entityType.Id;
                    rockContext.SaveChanges();
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.RestUserId] = restUser.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );

                return ActionOk( GetEntityBagForView( entity, rockContext ) );
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
                var entityService = new UserLoginService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<RestKeyBag, RestKeyDetailOptionsBag> box )
        {
            return ActionBadRequest( "Attributes are not supported by this block." );
        }

        /// <summary>
        /// Rreturn a random alpha numeric key while making sure the generated key doesn't match the filter
        /// </summary>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public string GenerateKey()
        {
            return Rock.Utility.KeyHelper.GenerateKey( ( RockContext rockContext, string key ) => new UserLoginService( rockContext ).Queryable().Any( a => a.ApiKey == key ) );
        }

        #endregion
    }
}
