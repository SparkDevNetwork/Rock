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
using Rock.ViewModels.Blocks.Cms.PersonalLinkSectionDetail;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular personal link section.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Personal Link Section Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a particular personal link section." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField(
        "Shared Section",
        Description = "When enabled, only shared sections will be displayed.",
        Key = AttributeKey.SharedSection,
        Order = 0 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "e76598f7-f686-41ee-848c-58e10758027f" )]
    [Rock.SystemGuid.BlockTypeGuid( "1abc8de5-a64d-4e69-875a-4407d9a7b425" )]
    public class PersonalLinkSectionDetail : RockDetailBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string PersonalLinkSectionId = "SectionId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class AttributeKey
        {
            public const string SharedSection = "SharedSection";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<PersonalLinkSectionBag, PersonalLinkSectionDetailOptionsBag>();

                SetBoxInitialEntityState( box, true, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<PersonalLinkSection>();

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
        private PersonalLinkSectionDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new PersonalLinkSectionDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the PersonalLinkSection for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="personalLinkSection">The PersonalLinkSection to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the PersonalLinkSection is valid, <c>false</c> otherwise.</returns>
        private bool ValidatePersonalLinkSection( PersonalLinkSection personalLinkSection, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<PersonalLinkSectionBag, PersonalLinkSectionDetailOptionsBag> box, bool loadAttributes, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext ) ?? new PersonalLinkSection { Id = 0 };

            var isViewable = entity.Id == 0 || entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.Id == 0 || entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( loadAttributes )
            {
                entity.LoadAttributes( rockContext );
            }

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity, loadAttributes );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                    box.Entity.CanAdministrate = entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( PersonalLinkSection.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity, loadAttributes );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( PersonalLinkSection.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="PersonalLinkSectionBag"/> that represents the entity.</returns>
        private PersonalLinkSectionBag GetCommonEntityBag( PersonalLinkSection entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new PersonalLinkSectionBag
            {
                IdKey = entity.IdKey,
                IsShared = entity.Id == 0 ? GetAttributeValue( AttributeKey.SharedSection ).AsBoolean() : entity.IsShared,
                Name = entity.Name
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <returns>A <see cref="PersonalLinkSectionBag"/> that represents the entity.</returns>
        private PersonalLinkSectionBag GetEntityBagForView( PersonalLinkSection entity, bool loadAttributes )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( loadAttributes )
            {
                bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );
            }

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <returns>A <see cref="PersonalLinkSectionBag"/> that represents the entity.</returns>
        private PersonalLinkSectionBag GetEntityBagForEdit( PersonalLinkSection entity, bool loadAttributes )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( loadAttributes )
            {
                bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );
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
        private bool UpdateEntityFromBox( PersonalLinkSection entity, DetailBlockBox<PersonalLinkSectionBag, PersonalLinkSectionDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

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
        /// <returns>The <see cref="PersonalLinkSection"/> to be viewed or edited on the page.</returns>
        private PersonalLinkSection GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<PersonalLinkSection, PersonalLinkSectionService>( rockContext, PageParameterKey.PersonalLinkSectionId );
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
        /// <param name="entity">The entity being viewed or edited on this block.</param>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( IHasAttributes entity )
        {
            return new Rock.Security.SecurityGrant()
                .AddRulesForAttributes( entity, RequestContext.CurrentPerson )
                .ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out PersonalLinkSection entity, out BlockActionResult error )
        {
            var entityService = new PersonalLinkSectionService( rockContext );
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
                entity = new PersonalLinkSection();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{PersonalLinkSection.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${PersonalLinkSection.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            using ( var rockContext = new RockContext() )
            {
                var breadCrumbs = new List<IBreadCrumb>();
                var key = pageReference.GetPageParameter( PageParameterKey.PersonalLinkSectionId );

                if ( !string.IsNullOrWhiteSpace( key ) )
                {
                    var pageParameters = new Dictionary<string, string>();
                    var name = new PersonalLinkSectionService( rockContext ).GetSelect( key, t => t.Name );

                    name = !string.IsNullOrWhiteSpace( name ) ? $"{name.FixCase()} Link Section" : "New Section";

                    pageParameters.Add( PageParameterKey.PersonalLinkSectionId, key );
                    var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
                    var breadCrumb = new BreadCrumbLink( name, breadCrumbPageRef );
                    breadCrumbs.Add( breadCrumb );
                }

                return new BreadCrumbResult
                {
                    BreadCrumbs = breadCrumbs
                };
            }
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

                var box = new DetailBlockBox<PersonalLinkSectionBag, PersonalLinkSectionDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, true )
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
        public BlockActionResult Save( DetailBlockBox<PersonalLinkSectionBag, PersonalLinkSectionDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new PersonalLinkSectionService( rockContext );
                var currentPerson = GetCurrentPerson();

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
                if ( !ValidatePersonalLinkSection( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                if ( isNew )
                {
                    var isShared = GetAttributeValue( AttributeKey.SharedSection ).AsBoolean();
                    entity.IsShared = isShared;

                    if ( !isShared )
                    {
                        entity.PersonAliasId = currentPerson.PrimaryAliasId;
                    }
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                entity = entityService.Get( entity.Id );

                if ( entity == null )
                {
                    return ActionInternalServerError( "Unable to save, please try again later." );
                }

                if ( entity.IsShared )
                {
                    var groupService = new GroupService( rockContext );

                    var communicationAdministrators = groupService.Get( Rock.SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS.AsGuid() );
                    if ( communicationAdministrators != null )
                    {
                        entity.AllowSecurityRole( Authorization.VIEW, communicationAdministrators, rockContext );
                        entity.AllowSecurityRole( Authorization.EDIT, communicationAdministrators, rockContext );
                        entity.AllowSecurityRole( Authorization.ADMINISTRATE, communicationAdministrators, rockContext );
                    }

                    var webAdministrators = groupService.Get( Rock.SystemGuid.Group.GROUP_WEB_ADMINISTRATORS.AsGuid() );
                    if ( webAdministrators != null )
                    {
                        entity.AllowSecurityRole( Authorization.VIEW, webAdministrators, rockContext );
                        entity.AllowSecurityRole( Authorization.EDIT, webAdministrators, rockContext );
                        entity.AllowSecurityRole( Authorization.ADMINISTRATE, webAdministrators, rockContext );
                    }

                    var rockAdministrators = groupService.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() );
                    if ( rockAdministrators != null )
                    {
                        entity.AllowSecurityRole( Authorization.VIEW, rockAdministrators, rockContext );
                        entity.AllowSecurityRole( Authorization.EDIT, rockAdministrators, rockContext );
                        entity.AllowSecurityRole( Authorization.ADMINISTRATE, rockAdministrators, rockContext );
                    }
                }
                else
                {
                    entity.MakePrivate( Authorization.VIEW, currentPerson );
                    entity.MakePrivate( Authorization.EDIT, currentPerson, rockContext );
                    entity.MakePrivate( Authorization.ADMINISTRATE, currentPerson, rockContext );
                }

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.PersonalLinkSectionId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );
                entity.LoadAttributes( rockContext );

                return ActionOk( GetEntityBagForView( entity, true ) );
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
                var entityService = new PersonalLinkSectionService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<PersonalLinkSectionBag, PersonalLinkSectionDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<PersonalLinkSectionBag, PersonalLinkSectionDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, true )
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

        #endregion
    }
}