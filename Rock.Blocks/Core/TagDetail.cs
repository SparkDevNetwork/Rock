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
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.TagDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular tag.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Tag Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a particular tag." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "919345d6-6e20-4501-b956-ebcb35d0b16e" )]
    [Rock.SystemGuid.BlockTypeGuid( "b150e767-e964-460c-9ed1-b293474c5f5d" )]
    public class TagDetail : RockEntityDetailBlockType<Tag, TagBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string TagId = "TagId";
            public const string EntityTypeId = "EntityTypeId";
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
            var box = new DetailBlockBox<TagBag, TagDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private TagDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new TagDetailOptionsBag();
            options.TagNameBlackListRegex = Tag.VALIDATOR_REGEX_BLACKLIST;
            return options;
        }

        /// <summary>
        /// Validates the Tag for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="tag">The Tag to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Tag is valid, <c>false</c> otherwise.</returns>
        private bool ValidateTag( Tag tag, out string errorMessage )
        {
            errorMessage = null;

            var entityService = new TagService( RockContext );
            var tagExists = entityService.Queryable()
                    .Where( t =>
                        t.Id != tag.Id &&
                        t.Name == tag.Name &&
                        (
                            ( t.OwnerPersonAlias == null && !tag.OwnerPersonAliasId.HasValue ) ||
                            ( t.OwnerPersonAlias != null && tag.OwnerPersonAliasId.HasValue && t.OwnerPersonAliasId == tag.OwnerPersonAliasId.Value )
                        ) &&
                        ( !t.EntityTypeId.HasValue || (
                            t.EntityTypeId.Value == tag.EntityTypeId &&
                            t.EntityTypeQualifierColumn == tag.EntityTypeQualifierColumn &&
                            t.EntityTypeQualifierValue == tag.EntityTypeQualifierValue )
                        ) )
                    .Any();

            if ( tagExists )
            {
                errorMessage = $"A '{tag.Name}' tag already exists for the selected scope, owner, and entity type.";
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
        private void SetBoxInitialEntityState( DetailBlockBox<TagBag, TagDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Tag.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = IsAuthorized( entity, Rock.Security.Authorization.VIEW );
            box.IsEditable = IsAuthorized( entity, Rock.Security.Authorization.EDIT );

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.SecurityGrantToken = GetSecurityGrant( entity ).ToToken();
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Tag.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );

                    if ( entity.Id == 0 )
                    {
                        var entityType = EntityTypeCache.Get( PageParameter( PageParameterKey.EntityTypeId ).AsInteger() );

                        if ( entityType != null )
                        {
                            box.Entity.EntityType = new ListItemBag() { Text = entityType.FriendlyName, Value = entityType.Guid.ToString() };
                        }
                    }
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Tag.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        private bool IsAuthorized( Tag entity, string action )
        {
            var currentPerson = GetCurrentPerson();
            var canConfigure = BlockCache.IsAuthorized( Rock.Security.Authorization.EDIT, currentPerson );
            return canConfigure || entity.IsAuthorized( action, currentPerson );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="TagBag"/> that represents the entity.</returns>
        private TagBag GetCommonEntityBag( Tag entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new TagBag
            {
                IdKey = entity.IdKey,
                BackgroundColor = entity.BackgroundColor,
                Category = entity.Category.ToListItemBag(),
                Description = entity.Description,
                EntityType = entity.EntityType.ToListItemBag(),
                EntityTypeQualifierColumn = entity.EntityTypeQualifierColumn,
                EntityTypeQualifierValue = entity.EntityTypeQualifierValue,
                IconCssClass = entity.IconCssClass,
                IsActive = entity.IsActive,
                Name = entity.Name,
                OwnerPersonAlias = entity.Id == 0 ? GetCurrentPerson().PrimaryAlias.ToListItemBag() : entity.OwnerPersonAlias.ToListItemBag(),
                CanAdministrate = IsAuthorized( entity, Rock.Security.Authorization.ADMINISTRATE )
            };
        }

        /// <inheritdoc/>
        protected override TagBag GetEntityBagForView( Tag entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override TagBag GetEntityBagForEdit( Tag entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( Tag entity, ValidPropertiesBox<TagBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.BackgroundColor ),
                () => entity.BackgroundColor = box.Bag.BackgroundColor );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IconCssClass ),
                () => entity.IconCssClass = box.Bag.IconCssClass );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            if ( BlockCache.IsAuthorized( Rock.Security.Authorization.EDIT, GetCurrentPerson() ) )
            {
                box.IfValidProperty( nameof( box.Bag.Category ),
                    () => entity.CategoryId = box.Bag.Category.GetEntityId<Category>( RockContext ) );

                box.IfValidProperty( nameof( box.Bag.EntityType ),
                    () => entity.EntityTypeId = box.Bag.EntityType.GetEntityId<EntityType>( RockContext ) );

                box.IfValidProperty( nameof( box.Bag.OwnerPersonAlias ),
                    () => entity.OwnerPersonAliasId = box.Bag.OwnerPersonAlias.GetEntityId<PersonAlias>( RockContext ) );

                box.IfValidProperty( nameof( box.Bag.EntityTypeQualifierColumn ),
                    () => entity.EntityTypeQualifierColumn = box.Bag.EntityTypeQualifierColumn );

                box.IfValidProperty( nameof( box.Bag.EntityTypeQualifierValue ),
                    () => entity.EntityTypeQualifierValue = box.Bag.EntityTypeQualifierValue );
            }

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override Tag GetInitialEntity()
        {
            return GetInitialEntity<Tag, TagService>( RockContext, PageParameterKey.TagId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out Tag entity, out BlockActionResult error )
        {
            var entityService = new TagService( RockContext );
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
                entity = new Tag();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Tag.FriendlyTypeName} not found." );
                return false;
            }

            if ( !IsAuthorized( entity, Rock.Security.Authorization.EDIT ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Tag.FriendlyTypeName}." );
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
                var key = pageReference.GetPageParameter( PageParameterKey.TagId );

                if ( !string.IsNullOrWhiteSpace( key ) )
                {
                    var pageParameters = new Dictionary<string, string>();
                    var name = new TagService( rockContext ).GetSelect( key, t => t.Name );

                    if ( name != null )
                    {
                        pageParameters.Add( PageParameterKey.TagId, key );
                        var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
                        var breadCrumb = new BreadCrumbLink( name, breadCrumbPageRef );
                        breadCrumbs.Add( breadCrumb );
                    }
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
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<TagBag>
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
        public BlockActionResult Save( ValidPropertiesBox<TagBag> box )
        {

            var entityService = new TagService( RockContext );

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
            if ( !ValidateTag( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.TagId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<TagBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new TagService( RockContext );

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

        #endregion
    }
}
