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
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "86759d9b-281c-4c1b-95e6-d4305731c03b" )]
    [Rock.SystemGuid.BlockTypeGuid( "96c5df9e-6f5c-4e55-92f1-61fe16a18563" )]
    public class AttributeMatrixTemplateDetail : RockDetailBlockType
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
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<AttributeMatrixTemplateBag, AttributeMatrixTemplateDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<AttributeMatrixTemplate>();

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
        private AttributeMatrixTemplateDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new AttributeMatrixTemplateDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the AttributeMatrixTemplate for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="attributeMatrixTemplate">The AttributeMatrixTemplate to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the AttributeMatrixTemplate is valid, <c>false</c> otherwise.</returns>
        private bool ValidateAttributeMatrixTemplate( AttributeMatrixTemplate attributeMatrixTemplate, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<AttributeMatrixTemplateBag, AttributeMatrixTemplateDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {AttributeMatrixTemplate.FriendlyTypeName} was not found.";
                return;
            }

            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

            box.Entity = GetEntityBag( entity, rockContext );
            box.SecurityGrantToken = GetSecurityGrantToken( entity );
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A <see cref="AttributeMatrixTemplateBag"/> that represents the entity.</returns>
        private AttributeMatrixTemplateBag GetEntityBag( AttributeMatrixTemplate entity, RockContext rockContext )
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

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );
            bag.TemplateAttributes = GetAttributes( entity.Id, rockContext ).ConvertAll( a => PublicAttributeHelper.GetPublicEditableAttributeViewModel( a ) );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( AttributeMatrixTemplate entity, DetailBlockBox<AttributeMatrixTemplateBag, AttributeMatrixTemplateDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.FormattedLava ),
                () => entity.FormattedLava = box.Entity.FormattedLava );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.MaximumRows ),
                () => entity.MaximumRows = box.Entity.MaximumRows.AsIntegerOrNull() );

            box.IfValidProperty( nameof( box.Entity.MinimumRows ),
                () => entity.MinimumRows = box.Entity.MinimumRows.AsIntegerOrNull() );

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
        /// <returns>The <see cref="AttributeMatrixTemplate"/> to be viewed or edited on the page.</returns>
        private AttributeMatrixTemplate GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<AttributeMatrixTemplate, AttributeMatrixTemplateService>( rockContext, PageParameterKey.AttributeMatrixTemplateId );
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
        private string GetSecurityGrantToken( AttributeMatrixTemplate entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out AttributeMatrixTemplate entity, out BlockActionResult error )
        {
            var entityService = new AttributeMatrixTemplateService( rockContext );
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
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<Rock.Model.Attribute> GetAttributes( int id, RockContext rockContext )
        {
            string qualifierValue = id.ToString();
            var attributeService = new AttributeService( rockContext );
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
        /// <param name="rockContext">The rock context.</param>
        private void SaveAttributes( List<PublicEditableAttributeBag> AttributesState, AttributeMatrixTemplate entity, RockContext rockContext )
        {
            /* Save Attributes */
            string qualifierValue = entity.Id.ToString();
            var entityTypeIdAttributeMatrix = EntityTypeCache.GetId<AttributeMatrixItem>();

            // Get the existing attributes for this entity type and qualifier value
            var attributeService = new AttributeService( rockContext );
            var regFieldService = new RegistrationTemplateFormFieldService( rockContext );
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
                rockContext.SaveChanges();
            }

            // Update the Attributes that were assigned in the UI
            foreach ( var attributeState in AttributesState )
            {
                Helper.SaveAttributeEdits( attributeState, entityTypeIdAttributeMatrix, "AttributeMatrixTemplateId", qualifierValue, rockContext );
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
            using ( var rockContext = new RockContext() )
            {
                // Get the queryable and make sure it is ordered correctly.
                var id = Rock.Utility.IdHasher.Instance.GetId( idKey );

                var attributes = GetAttributes( id ?? 0, rockContext );

                if ( !attributes.ReorderEntity( guid.ToString(), beforeGuid.ToString() ) )
                {
                    return ActionBadRequest( "Invalid reorder attempt." );
                }

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<AttributeMatrixTemplateBag, AttributeMatrixTemplateDetailOptionsBag> box )
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
                if ( !ValidateAttributeMatrixTemplate( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );

                    SaveAttributes( box.Entity.TemplateAttributes, entity, rockContext );
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
        public BlockActionResult RefreshAttributes( DetailBlockBox<AttributeMatrixTemplateBag, AttributeMatrixTemplateDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<AttributeMatrixTemplateBag, AttributeMatrixTemplateDetailOptionsBag>
                {
                    Entity = GetEntityBag( entity, rockContext )
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
