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
using Rock.ViewModels.Blocks.Core.AttributeMatrixTemplateDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular attribute matrix template.
    /// </summary>
    [DisplayName( "Attribute Matrix Template Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a particular attribute matrix template." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "86759d9b-281c-4c1b-95e6-d4305731c03b" )]
    [Rock.SystemGuid.BlockTypeGuid( "96c5df9e-6f5c-4e55-92f1-61fe16a18563" )]
    public class AttributeMatrixTemplateDetail : RockEntityDetailBlockType<AttributeMatrixTemplate, AttributeMatrixTemplateBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string AttributeMatrixTemplateId = "AttributeMatrixTemplateId";
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
            var box = new DetailBlockBox<AttributeMatrixTemplateBag, AttributeMatrixTemplateDetailOptionsBag>();

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
        /// <returns>The options that provide additional details to the block.</returns>
        private AttributeMatrixTemplateDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new AttributeMatrixTemplateDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the AttributeMatrixTemplate for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="attributeMatrixTemplate">The AttributeMatrixTemplate to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the AttributeMatrixTemplate is valid, <c>false</c> otherwise.</returns>
        private bool ValidateAttributeMatrixTemplate( AttributeMatrixTemplate attributeMatrixTemplate, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<AttributeMatrixTemplateBag, AttributeMatrixTemplateDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {AttributeMatrixTemplate.FriendlyTypeName} was not found.";
                return;
            }

            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( RockContext );
            box.Entity = GetEntityBagForEdit( entity );
            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="AttributeMatrixTemplateBag"/> that represents the entity.</returns>
        private AttributeMatrixTemplateBag GetCommonEntityBag( AttributeMatrixTemplate entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new AttributeMatrixTemplateBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                FormattedLava = entity.Id == 0 ? AttributeMatrixTemplate.FormattedLavaDefault : entity.FormattedLava,
                IsActive = entity.IsActive,
                MaximumRows = entity.MaximumRows.ToString(),
                MinimumRows = entity.MinimumRows.ToString(),
                Name = entity.Name
            };

            bag.TemplateAttributes = GetAttributes( entity.Id ).ConvertAll( a => PublicAttributeHelper.GetPublicEditableAttributeViewModel( a ) );

            return bag;
        }

        // <inheritdoc/>
        protected override AttributeMatrixTemplateBag GetEntityBagForEdit( AttributeMatrixTemplate entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: false );

            return bag;
        }

        // <inheritdoc/>
        protected override AttributeMatrixTemplateBag GetEntityBagForView( AttributeMatrixTemplate entity )
        {
            return GetEntityBagForEdit( entity );
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( AttributeMatrixTemplate entity, ValidPropertiesBox<AttributeMatrixTemplateBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.FormattedLava ),
                () => entity.FormattedLava = box.Bag.FormattedLava );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.MaximumRows ),
                () => entity.MaximumRows = box.Bag.MaximumRows.AsIntegerOrNull() );

            box.IfValidProperty( nameof( box.Bag.MinimumRows ),
                () => entity.MinimumRows = box.Bag.MinimumRows.AsIntegerOrNull() );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: false );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override AttributeMatrixTemplate GetInitialEntity()
        {
            return GetInitialEntity<AttributeMatrixTemplate, AttributeMatrixTemplateService>( RockContext, PageParameterKey.AttributeMatrixTemplateId );
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

        // <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out AttributeMatrixTemplate entity, out BlockActionResult error )
        {
            var entityService = new AttributeMatrixTemplateService( RockContext );
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
                entity = new AttributeMatrixTemplate();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{AttributeMatrixTemplate.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${AttributeMatrixTemplate.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the attributes for the AttributeMatrixTemplate entity.
        /// </summary>
        /// <param name="id">The entity identifier.</param>
        /// <returns></returns>
        private List<Rock.Model.Attribute> GetAttributes( int id )
        {
            string qualifierValue = id.ToString();
            var attributeService = new AttributeService( RockContext );
            return attributeService.GetByEntityTypeQualifier( new AttributeMatrixItem().TypeId, "AttributeMatrixTemplateId", qualifierValue, true ).AsQueryable()
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        /// <summary>
        /// Saves the attributes.
        /// </summary>
        /// <param name="AttributesState">State of the attributes.</param>
        /// <param name="entity">The entity.</param>
        private void SaveAttributes( List<PublicEditableAttributeBag> AttributesState, AttributeMatrixTemplate entity )
        {
            /* Save Attributes */
            string qualifierValue = entity.Id.ToString();
            var entityTypeIdAttributeMatrix = EntityTypeCache.GetId<AttributeMatrixItem>();

            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( RockContext );
            var regFieldService = new RegistrationTemplateFormFieldService( RockContext );
            var attributes = attributeService.GetByEntityTypeQualifier( entityTypeIdAttributeMatrix, "AttributeMatrixTemplateId", qualifierValue, true );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = AttributesState.Select( a => a.Guid );
            foreach ( var attr in attributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                foreach ( var field in regFieldService.Queryable().Where( f => f.AttributeId.HasValue && f.AttributeId.Value == attr.Id ).ToList() )
                {
                    regFieldService.Delete( field );
                }

                attributeService.Delete( attr );
                RockContext.SaveChanges();
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attributeState in AttributesState )
            {
                Helper.SaveAttributeEdits( attributeState, entityTypeIdAttributeMatrix, "AttributeMatrixTemplateId", qualifierValue, RockContext );
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Changes the ordered position of a single item.
        /// </summary>
        /// <param name="key">The identifier of the item that will be moved.</param>
        /// <param name="beforeKey">The identifier of the item it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderAttributes( string idKey, Guid guid, Guid? beforeGuid )
        {
            // Get the queryable and make sure it is ordered correctly.
            var id = Rock.Utility.IdHasher.Instance.GetId( idKey );

            var attributes = GetAttributes( id ?? 0 );

            if ( !attributes.ReorderEntity( guid.ToString(), beforeGuid.ToString() ) )
            {
                return ActionBadRequest( "Invalid reorder attempt." );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<AttributeMatrixTemplateBag> box )
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
            if ( !ValidateAttributeMatrixTemplate( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );

                SaveAttributes( box.Bag.TemplateAttributes, entity );
            } );

            return ActionOk( this.GetParentPageUrl() );
        }

        #endregion
    }
}
