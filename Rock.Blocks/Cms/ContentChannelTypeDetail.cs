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
using System.Net.Mime;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.ContentChannelTypeDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular content channel type.
    /// </summary>

    [DisplayName( "Content Channel Type Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details for a content channel type." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "1e0caf78-d33a-45b8-91db-7a435158f98a" )]
    [Rock.SystemGuid.BlockTypeGuid( "2ad9e6bc-f764-4374-a714-53e365d77a36" )]
    public class ContentChannelTypeDetail : RockDetailBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string ContentChannelTypeId = "TypeId";
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
                var box = new DetailBlockBox<ContentChannelTypeBag, ContentChannelTypeDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );

                return box;
            }
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            using ( var rockContext = new RockContext() )
            {
                var contentTypeId = pageReference.GetPageParameter( PageParameterKey.ContentChannelTypeId );
                var contentTypeName = new ContentChannelTypeService( rockContext )
                    .GetSelect( contentTypeId, b => b.Name );
                var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageReference.Parameters );
                var breadCrumb = new BreadCrumbLink( contentTypeName ?? "New Content Type", breadCrumbPageRef );

                return new BreadCrumbResult
                {
                    BreadCrumbs = new List<IBreadCrumb>
                    {
                       breadCrumb
                    }
                };
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private ContentChannelTypeDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new ContentChannelTypeDetailOptionsBag();
            options.DateRangeTypes = typeof( ContentChannelDateType ).ToEnumListItemBag();
            return options;
        }

        /// <summary>
        /// Validates the ContentChannelType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="contentChannelType">The ContentChannelType to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the ContentChannelType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateContentChannelType( ContentChannelType contentChannelType, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            if ( !contentChannelType.IsValid )
            {
                errorMessage = contentChannelType.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
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
        private void SetBoxInitialEntityState( DetailBlockBox<ContentChannelTypeBag, ContentChannelTypeDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {ContentChannelType.FriendlyTypeName} was not found.";
                return;
            }

            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            // New entity is being created, prepare for edit mode by default.
            if ( box.IsEditable )
            {
                box.Entity = GetEntityBagForEdit( entity, rockContext );
                box.SecurityGrantToken = GetSecurityGrantToken( entity );
            }
            else
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( ContentChannelType.FriendlyTypeName );
            }
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="ContentChannelTypeBag"/> that represents the entity.</returns>
        private ContentChannelTypeBag GetEntityBagForEdit( ContentChannelType entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new ContentChannelTypeBag
            {
                IdKey = entity.IdKey,
                DateRangeType = entity.DateRangeType.ConvertToInt(),
                DisableContentField = entity.DisableContentField,
                DisablePriority = entity.DisablePriority,
                DisableStatus = entity.DisableStatus,
                IncludeTime = entity.IncludeTime,
                IsSystem = entity.IsSystem,
                Name = entity.Name,
                ShowInChannelList = entity.ShowInChannelList
            };

            bag.ItemAttributes = GetAttributes( rockContext, entity.Id, new ContentChannelItem().TypeId ).ConvertAll( a => PublicAttributeHelper.GetPublicEditableAttributeViewModel( a ) );
            bag.ChannelAttributes = GetAttributes( rockContext, entity.Id, new ContentChannel().TypeId ).ConvertAll( a => PublicAttributeHelper.GetPublicEditableAttributeViewModel( a ) );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( ContentChannelType entity, DetailBlockBox<ContentChannelTypeBag, ContentChannelTypeDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.DateRangeType ),
                () => entity.DateRangeType = ( ContentChannelDateType ) box.Entity.DateRangeType );

            box.IfValidProperty( nameof( box.Entity.DisableContentField ),
                () => entity.DisableContentField = box.Entity.DisableContentField );

            box.IfValidProperty( nameof( box.Entity.DisablePriority ),
                () => entity.DisablePriority = box.Entity.DisablePriority );

            box.IfValidProperty( nameof( box.Entity.DisableStatus ),
                () => entity.DisableStatus = box.Entity.DisableStatus );

            box.IfValidProperty( nameof( box.Entity.IncludeTime ),
                () => entity.IncludeTime = box.Entity.IncludeTime );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.ShowInChannelList ),
                () => entity.ShowInChannelList = box.Entity.ShowInChannelList );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="ContentChannelType"/> to be viewed or edited on the page.</returns>
        private ContentChannelType GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<ContentChannelType, ContentChannelTypeService>( rockContext, PageParameterKey.ContentChannelTypeId );
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
        private string GetSecurityGrantToken( ContentChannelType entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out ContentChannelType entity, out BlockActionResult error )
        {
            var entityService = new ContentChannelTypeService( rockContext );
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
                entity = new ContentChannelType();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{ContentChannelType.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${ContentChannelType.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the attributes for the Content Channel Type.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="qualifierValue">The qualifier value.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        public List<Rock.Model.Attribute> GetAttributes( RockContext rockContext, int qualifierValue, int entityTypeId )
        {
            var attributeService = new AttributeService( rockContext );
            return attributeService.GetByEntityTypeId( entityTypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( qualifierValue.ToString() ) )
                .OrderBy( a => a.Order )
                .ToList();
        }

        /// <summary>
        /// Saves the attributes.
        /// </summary>
        /// <param name="contentTypeId">The content type identifier.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SaveAttributes( int contentTypeId, int entityTypeId, List<PublicEditableAttributeBag> attributes, RockContext rockContext )
        {
            string qualifierColumn = "ContentChannelTypeId";
            string qualifierValue = contentTypeId.ToString();

            AttributeService attributeService = new AttributeService( rockContext );

            // Get the existing attributes for this entity type and qualifier value
            var existingAttributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = attributes.Select( a => a.Guid );
            foreach ( var attr in existingAttributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
            }

            rockContext.SaveChanges();

            // Update the Attributes that were assigned in the UI
            foreach ( var attr in attributes )
            {
                Rock.Attribute.Helper.SaveAttributeEdits( attr, entityTypeId, qualifierColumn, qualifierValue, rockContext );
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

                var box = new DetailBlockBox<ContentChannelTypeBag, ContentChannelTypeDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<ContentChannelTypeBag, ContentChannelTypeDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new ContentChannelTypeService( rockContext );

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
                if ( !ValidateContentChannelType( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                if ( entity.Id == 0 )
                {
                    entity.CreatedByPersonAliasId = GetCurrentPerson()?.PrimaryAliasId;
                    entity.CreatedDateTime = RockDateTime.Now;
                }
                else
                {
                    entity.ModifiedByPersonAliasId = GetCurrentPerson()?.PrimaryAliasId; ;
                    entity.ModifiedDateTime = RockDateTime.Now;
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    // get it back to make sure we have a good Id for it for the Attributes
                    entity = entityService.Get( entity.Guid );

                    // Save the Channel Attributes
                    int entityTypeId = EntityTypeCache.Get( typeof( ContentChannel ) ).Id;
                    SaveAttributes( entity.Id, entityTypeId, box.Entity.ChannelAttributes, rockContext );

                    // Save the Item Attributes
                    entityTypeId = EntityTypeCache.Get( typeof( ContentChannelItem ) ).Id;
                    SaveAttributes( entity.Id, entityTypeId, box.Entity.ItemAttributes, rockContext );
                } );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<ContentChannelTypeBag, ContentChannelTypeDetailOptionsBag> box )
        {
            return ActionBadRequest( "Attributes are not supported by this block." );
        }

        /// <summary>
        /// Reorders the Item attributes.
        /// </summary>
        /// <param name="idKey">The identifier key.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="beforeGuid">The before unique identifier.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult ReorderItemAttributes( string idKey, Guid guid, Guid? beforeGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                // Get the queryable and make sure it is ordered correctly.
                var id = Rock.Utility.IdHasher.Instance.GetId( idKey );

                var attributes = GetAttributes( rockContext, id ?? 0, new ContentChannelItem().TypeId );

                if ( !attributes.ReorderEntity( guid.ToString(), beforeGuid.ToString() ) )
                {
                    return ActionBadRequest( "Invalid reorder attempt." );
                }

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Reorders the channel attributes.
        /// </summary>
        /// <param name="idKey">The identifier key.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="beforeGuid">The before unique identifier.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult ReorderChannelAttributes( string idKey, Guid guid, Guid? beforeGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                // Get the queryable and make sure it is ordered correctly.
                var id = Rock.Utility.IdHasher.Instance.GetId( idKey );

                var attributes = GetAttributes( rockContext, id ?? 0, new ContentChannel().TypeId );

                if ( !attributes.ReorderEntity( guid.ToString(), beforeGuid.ToString() ) )
                {
                    return ActionBadRequest( "Invalid reorder attempt." );
                }

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion
    }
}
