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

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Prayer.PrayerRequestDetail;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Rock.Blocks.Prayer
{
    /// <summary>
    /// Displays the details of a particular prayer request.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Prayer Request Detail" )]
    [Category( "Prayer" )]
    [Description( "Displays the details of a particular prayer request." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [IntegerField( "Expires After (days)",
        Key = AttributeKey.ExpireDays,
        Description = "Default number of days until the request will expire.",
        IsRequired = false,
        DefaultIntegerValue = 14,
        Category = "",
        Order = 0 )]

    [CategoryField( "Default Category",
        Description = "If a category is not selected, choose a default category to use for all new prayer requests.",
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.PrayerRequest",
        EntityTypeQualifierColumn = "",
        EntityTypeQualifierValue = "",
        IsRequired = false,
        DefaultValue = "4B2D88F5-6E45-4B4B-8776-11118C8E8269",
        Category = "",
        Order = 1,
        Key = AttributeKey.DefaultCategory )]

    [BooleanField( "Set Current Person To Requester",
        Description = "Will set the current person as the requester. This is useful in self-entry situations.",
        DefaultBooleanValue = false,
        Order = 2 )]

    [BooleanField( "Require Last Name",
        Description = "Require that a last name be entered",
        DefaultBooleanValue = true,
        Category = "",
        Order = 3 )]

    [BooleanField( "Default To Public",
        Description = "If enabled, all prayers will be set to public by default",
        DefaultBooleanValue = false,
        Category = "",
        Order = 4 )]

    [BooleanField( "Default Allow Comments Checked",
        Description = "If true, the Allow Comments checkbox will be pre-checked for all new requests by default.",
        DefaultBooleanValue = true,
        Order = 5 )]

    [BooleanField( "Require Campus",
        Description = "Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.",
        DefaultBooleanValue = false,
        Category = "",
        Order = 6 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "d1e21128-c831-4535-b8df-0ec928dcbba4" )]
    [Rock.SystemGuid.BlockTypeGuid( "e120f06f-6db7-464a-a797-c3c90b92ef40" )]
    public class PrayerRequestDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string PrayerRequestId = "PrayerRequestId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string RequireLastName = "RequireLastName";
            public const string RequireCampus = "RequireCampus";
            public const string DefaultToPublic = "DefaultToPublic";
            public const string DefaultAllowCommentsChecked = "DefaultAllowCommentsChecked";
            public const string SetCurrentPersonToRequester = "SetCurrentPersonToRequester";
            public const string DefaultCategory = "DefaultCategory";
            public const string ExpireDays = "ExpireDays";
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<PrayerRequestBag, PrayerRequestDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<PrayerRequest>();

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
        private PrayerRequestDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new PrayerRequestDetailOptionsBag
            {
                IsLastNameRequired = GetAttributeValue( AttributeKey.RequireLastName ).AsBooleanOrNull() ?? true,
                IsCampusRequired = GetAttributeValue( AttributeKey.RequireCampus ).AsBooleanOrNull() ?? false
            };
            return options;
        }

        /// <summary>
        /// Validates the PrayerRequest for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="prayerRequest">The PrayerRequest to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the PrayerRequest is valid, <c>false</c> otherwise.</returns>
        private bool ValidatePrayerRequest( PrayerRequest prayerRequest, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            return prayerRequest.IsValid;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<PrayerRequestBag, PrayerRequestDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {PrayerRequest.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( PrayerRequest.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );

                    box.Entity.IsApproved = true;

                    // set the default value for new prayer request from the block properties
                    box.Entity.AllowComments = GetAttributeValue( AttributeKey.DefaultAllowCommentsChecked ).AsBooleanOrNull() ?? true;
                    box.Entity.IsPublic = GetAttributeValue( AttributeKey.DefaultToPublic ).AsBoolean();

                    // if default the requester to the current person based on the block attribute
                    var CurrentPerson = this.GetCurrentPerson();
                    if ( CurrentPerson != null && GetAttributeValue( AttributeKey.SetCurrentPersonToRequester ).AsBoolean() )
                    {
                        box.Entity.RequestedByPersonAlias = CurrentPerson.PrimaryAlias.ToListItemBag();
                        box.Entity.FirstName = CurrentPerson.NickName;
                        box.Entity.LastName = CurrentPerson.LastName;
                    }

                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( PrayerRequest.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="PrayerRequestBag"/> that represents the entity.</returns>
        private PrayerRequestBag GetCommonEntityBag( PrayerRequest entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new PrayerRequestBag
            {
                IdKey = entity.IdKey,
                AllowComments = entity.AllowComments,
                Answer = entity.Answer,
                Campus = entity.Campus.ToListItemBag(),
                Category = entity.Category.ToListItemBag(),
                Email = entity.Email,
                ExpirationDate = entity.ExpirationDate,
                FirstName = entity.FirstName,
                FlagCount = entity.FlagCount,
                Group = entity.Group.ToListItemBag(),
                IsActive = entity.IsActive,
                IsApproved = entity.IsApproved,
                IsPublic = entity.IsPublic,
                IsUrgent = entity.IsUrgent,
                LastName = entity.LastName,
                PrayerCount = entity.PrayerCount,
                RequestedByPersonAlias = entity.RequestedByPersonAlias.ToListItemBag(),
                Text = entity.Text,
                FullName = entity.FullName
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="PrayerRequestBag"/> that represents the entity.</returns>
        private PrayerRequestBag GetEntityBagForView( PrayerRequest entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            bag.Text = entity.Text.ScrubHtmlAndConvertCrLfToBr();
            bag.Answer = entity.Answer.ScrubHtmlAndConvertCrLfToBr();
            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="PrayerRequestBag"/> that represents the entity.</returns>
        private PrayerRequestBag GetEntityBagForEdit( PrayerRequest entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            // If there is no email for the prayer request detail entity, set it to the requester's 
            if ( string.IsNullOrWhiteSpace( entity.Email ) && entity.RequestedByPersonAlias != null )
            {
                bag.Email = entity.RequestedByPersonAlias.Person.Email;
            }

            // get the experiation date from the block attributes if the prayer request has no expiration date set
            if ( entity.ExpirationDate == null )
            {
                var expireDays = Convert.ToDouble( GetAttributeValue( AttributeKey.ExpireDays ) );
                bag.ExpirationDate = RockDateTime.Now.AddDays( expireDays );
            }

            // set to the default category in edit mode if no category specified on the prayer request
            if ( entity.Category == null )
            {
                bag.Category = CategoryCache.Get( GetAttributeValue( AttributeKey.DefaultCategory ) ).ToListItemBag();
            }

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( PrayerRequest entity, DetailBlockBox<PrayerRequestBag, PrayerRequestDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.AllowComments ),
                () => entity.AllowComments = box.Entity.AllowComments );

            box.IfValidProperty( nameof( box.Entity.Answer ),
                () => entity.Answer = box.Entity.Answer );

            box.IfValidProperty( nameof( box.Entity.Campus ),
                () => entity.CampusId = box.Entity.Campus.GetEntityId<Campus>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.Category ),
                () => entity.CategoryId = box.Entity.Category.GetEntityId<Category>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.Email ),
                () => entity.Email = box.Entity.Email );

            box.IfValidProperty( nameof( box.Entity.ExpirationDate ),
                () => entity.ExpirationDate = box.Entity.ExpirationDate );

            box.IfValidProperty( nameof( box.Entity.FirstName ),
                () => entity.FirstName = box.Entity.FirstName );

            box.IfValidProperty( nameof( box.Entity.FlagCount ),
                () => entity.FlagCount = box.Entity.FlagCount );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.IsApproved ),
                () => entity.IsApproved = box.Entity.IsApproved );

            box.IfValidProperty( nameof( box.Entity.IsPublic ),
                () => entity.IsPublic = box.Entity.IsPublic );

            box.IfValidProperty( nameof( box.Entity.IsUrgent ),
                () => entity.IsUrgent = box.Entity.IsUrgent );

            box.IfValidProperty( nameof( box.Entity.LastName ),
                () => entity.LastName = box.Entity.LastName );

            box.IfValidProperty( nameof( box.Entity.PrayerCount ),
                () => entity.PrayerCount = box.Entity.PrayerCount );

            box.IfValidProperty( nameof( box.Entity.RequestedByPersonAlias ),
                () => entity.RequestedByPersonAliasId = box.Entity.RequestedByPersonAlias.GetEntityId<PersonAlias>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.Text ),
                () => entity.Text = box.Entity.Text );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="PrayerRequest"/> to be viewed or edited on the page.</returns>
        private PrayerRequest GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<PrayerRequest, PrayerRequestService>( rockContext, PageParameterKey.PrayerRequestId );
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
        private string GetSecurityGrantToken( PrayerRequest entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out PrayerRequest entity, out BlockActionResult error )
        {
            var entityService = new PrayerRequestService( rockContext );
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
                entity = new PrayerRequest();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{PrayerRequest.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${PrayerRequest.FriendlyTypeName}." );
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

                var box = new DetailBlockBox<PrayerRequestBag, PrayerRequestDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Get the Nick Name, the Last Name and the email of the person from the database given the Person Alias Guid
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult GetPersonName( string personAliasGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var personAliasService = new PersonAliasService( rockContext );
                var person = personAliasService.Get( personAliasGuid ).Person;


                return ActionOk( new PrayerRequestDetailAddPersonResponseBag
                {
                    nickName = person.NickName,
                    lastName = person.LastName,
                    email = person.Email
                } )
                ;
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<PrayerRequestBag, PrayerRequestDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new PrayerRequestService( rockContext );

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
                if ( !ValidatePrayerRequest( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetParentPageUrl() );
                }

                return ActionContent( System.Net.HttpStatusCode.OK, this.GetParentPageUrl() );
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
                var entityService = new PrayerRequestService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }
                DeleteAllRelatedNotes( entity, rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<PrayerRequestBag, PrayerRequestDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<PrayerRequestBag, PrayerRequestDetailOptionsBag>
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
        /// Deletes all related notes for a prayer request.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        /// <param name="rockContext">The Rock Context.</param>
        private void DeleteAllRelatedNotes( PrayerRequest prayerRequest, RockContext rockContext )
        {
            var prayerRequestEntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.PRAYER_REQUEST.AsGuid() ).Id;
            var noteTypeIdsForPrayerRequest = EntityNoteTypesCache.Get()
                .EntityNoteTypes
                .First( a => a.EntityTypeId.Equals( prayerRequestEntityTypeId ) )
                .NoteTypeIds;
            var noteService = new NoteService( rockContext );
            var prayerRequestComments = noteService.Queryable()
                .Where( n => noteTypeIdsForPrayerRequest.Contains( n.NoteTypeId ) && n.EntityId == prayerRequest.Id );
            rockContext.BulkDelete( prayerRequestComments );
        }

        #endregion
    }
}
