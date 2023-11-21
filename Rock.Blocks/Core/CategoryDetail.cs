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
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.CategoryDetail;
using Rock.ViewModels.Cms;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular category.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Category Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a particular category." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes
    [EntityTypeField( "Entity Type",
        Description = "The type of entity to associate category with",
        Key = AttributeKey.EntityType )]

    [TextField( "Entity Type Qualifier Property",
        IsRequired = false,
        Key = AttributeKey.EntityTypeQualifierProperty )]

    [TextField( "Entity Type Qualifier Value",
        IsRequired = false,
        Key = AttributeKey.EntityTypeQualifierValue )]

    [CategoryField( "Root Category",
        Description = "Select the root category to use as a starting point for the parent category picker.",
        AllowMultiple = false,
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.RootCategory )]

    [CategoryField( "Exclude Categories",
        Description = "Select any category that you need to exclude from the parent category picker",
        AllowMultiple = true,
        IsRequired = false,
        Category = "CustomSetting",
        Key = AttributeKey.ExcludeCategories )]
    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "2889352c-52ba-45f6-8ee1-9afa61211582" )]
    [Rock.SystemGuid.BlockTypeGuid( "515dc5c2-4fbd-4eea-9d8e-a807409defde" )]
    public class CategoryDetail : RockDetailBlockType, IHasCustomActions
    {
        #region Keys

        private static class AttributeKey
        {
            public const string EntityType = "EntityType";

            public const string EntityTypeQualifierProperty = "EntityTypeQualifierProperty";

            public const string EntityTypeQualifierValue = "EntityTypeQualifierValue";

            public const string RootCategory = "RootCategory";

            public const string ExcludeCategories = "ExcludeCategories";
        }

        private static class PageParameterKey
        {
            public const string CategoryId = "CategoryId";
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
                var box = new DetailBlockBox<CategoryBag, CategoryDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<Category>();

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
        private CategoryDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new CategoryDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the Category for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="category">The Category to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Category is valid, <c>false</c> otherwise.</returns>
        private bool ValidateCategory( Category category, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            if ( category.EntityTypeId == 0 )
            {
                errorMessage = "An EntityType was not configured for this block. Please contact your system administrator for assistance. <br />";
                return false;
            }

            // if the category IsValid is false, and the UI controls didn't report any errors, it is probably because the custom rules of category didn't pass.
            // So, make sure a message is displayed in the validation summary
            if ( !category.IsValid )
            {
                errorMessage = category.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
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
        private void SetBoxInitialEntityState( DetailBlockBox<CategoryBag, CategoryDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Category.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Category.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Category.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="CategoryBag"/> that represents the entity.</returns>
        private CategoryBag GetCommonEntityBag( Category entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new CategoryBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                EntityType = entity.EntityType.ToListItemBag(),
                EntityTypeQualifierColumn = entity.EntityTypeQualifierColumn,
                EntityTypeQualifierValue = entity.EntityTypeQualifierValue,
                HighlightColor = entity.HighlightColor,
                IconCssClass = entity.IconCssClass,
                IsSystem = entity.IsSystem,
                Name = entity.Name,
                ParentCategory = entity.ParentCategory.ToListItemBag()
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="CategoryBag"/> that represents the entity.</returns>
        private CategoryBag GetEntityBagForView( Category entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="CategoryBag"/> that represents the entity.</returns>
        private CategoryBag GetEntityBagForEdit( Category entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( entity.Id == 0 )
            {
                bag.EntityTypeQualifierColumn = GetAttributeValue( AttributeKey.EntityTypeQualifierProperty );
                bag.EntityTypeQualifierValue = GetAttributeValue( AttributeKey.EntityTypeQualifierProperty );
                bag.RootCategoryGuid = GetAttributeValue( AttributeKey.RootCategory ).AsGuidOrNull();
                var entityTypeGuid = GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull();
                if ( entityTypeGuid.HasValue )
                {
                    var entityType = EntityTypeCache.Get( entityTypeGuid.Value );
                    bag.EntityType = new ViewModels.Utility.ListItemBag
                    {
                        Text = entityType.Name,
                        Value = entityType.Guid.ToString()
                    };
                }
            }

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( Category entity, DetailBlockBox<CategoryBag, CategoryDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.HighlightColor ),
                () => entity.HighlightColor = box.Entity.HighlightColor );

            box.IfValidProperty( nameof( box.Entity.IconCssClass ),
                () => entity.IconCssClass = box.Entity.IconCssClass );

            box.IfValidProperty( nameof( box.Entity.IsSystem ),
                () => entity.IsSystem = box.Entity.IsSystem );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.ParentCategory ),
                () => entity.ParentCategoryId = box.Entity.ParentCategory.GetEntityId<Category>( rockContext ) );

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
        /// <returns>The <see cref="Category"/> to be viewed or edited on the page.</returns>
        private Category GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<Category, CategoryService>( rockContext, PageParameterKey.CategoryId );
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
        private string GetSecurityGrantToken( Category entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out Category entity, out BlockActionResult error )
        {
            var entityService = new CategoryService( rockContext );
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
                entity = new Category();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Category.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Category.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken()
        {
            return new Rock.Security.SecurityGrant()
                .ToToken();
        }

        #endregion

        #region IHasCustomActions

        /// <inheritdoc/>
        List<BlockCustomActionBag> IHasCustomActions.GetCustomActions( bool canEdit, bool canAdministrate )
        {
            var actions = new List<BlockCustomActionBag>();

            if ( canAdministrate )
            {
                actions.Add( new BlockCustomActionBag
                {
                    IconCssClass = "fa fa-edit",
                    Tooltip = "Settings",
                    ComponentFileUrl = "/Obsidian/Blocks/Core/categoryDetailCustomSettings.obs"
                } );
            }

            return actions;
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

                var box = new DetailBlockBox<CategoryBag, CategoryDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<CategoryBag, CategoryDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new CategoryService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                var isNew = entity.Id == 0;
                if ( isNew )
                {
                    if ( Guid.TryParse( GetAttributeValue( AttributeKey.EntityType ), out Guid entityTypeGuid ) )
                    {
                        entity.EntityTypeId = EntityTypeCache.Get( entityTypeGuid ).Id;
                    }
                    entity.EntityTypeQualifierColumn = GetAttributeValue( AttributeKey.EntityTypeQualifierProperty );
                    entity.EntityTypeQualifierValue = GetAttributeValue( AttributeKey.EntityTypeQualifierValue );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateCategory( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.CategoryId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );
                entity.LoadAttributes( rockContext );

                return ActionOk( GetEntityBagForView( entity ) );
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
                var entityService = new CategoryService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<CategoryBag, CategoryDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<CategoryBag, CategoryDetailOptionsBag>
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
        /// Gets the values and all other required details that will be needed
        /// to display the custom settings modal.
        /// </summary>
        /// <returns>A box that contains the custom settings values and additional data.</returns>
        [BlockAction]
        public BlockActionResult GetCustomSettings()
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "Not authorized to edit block settings." );
                }

                var options = new CustomSettingsOptionsBag();
                var rootCategoryGuid = GetAttributeValue( AttributeKey.RootCategory ).AsGuidOrNull();
                var excludeCategoryGuids = GetAttributeValue( AttributeKey.ExcludeCategories ).SplitDelimitedValues().AsGuidList();
                var categoryGuids = excludeCategoryGuids.ToList();
                if ( rootCategoryGuid.HasValue )
                {
                    categoryGuids.Add( rootCategoryGuid.Value );
                }
                var categories = new CategoryService( rockContext ).GetByGuids( categoryGuids );
                var settings = new CustomSettingsBag
                {
                    RootCategory = rootCategoryGuid.HasValue ? categories.FirstOrDefault( a => a.Guid == rootCategoryGuid.Value ).ToListItemBag() : null,
                    ExcludeCategories = categories.Where( a => excludeCategoryGuids.Contains( a.Guid ) ).ToListItemBagList(),
                    EntityTypeGuid = GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull()
                };

                return ActionOk( new CustomSettingsBox<CustomSettingsBag, CustomSettingsOptionsBag>
                {
                    Settings = settings,
                    Options = options,
                    SecurityGrantToken = GetSecurityGrantToken()
                } );
            }
        }

        /// <summary>
        /// Saves the updates to the custom setting values for this block.
        /// </summary>
        /// <param name="box">The box that contains the setting values.</param>
        /// <returns>A response that indicates if the save was successful or not.</returns>
        [BlockAction]
        public BlockActionResult SaveCustomSettings( CustomSettingsBox<CustomSettingsBag, CustomSettingsOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "Not authorized to edit block settings." );
                }

                var block = new BlockService( rockContext ).Get( BlockId );
                block.LoadAttributes( rockContext );

                box.IfValidProperty( nameof( box.Settings.RootCategory ),
                    () => block.SetAttributeValue( AttributeKey.RootCategory, box.Settings.RootCategory?.Value ) );

                box.IfValidProperty( nameof( box.Settings.ExcludeCategories ),
                    () => block.SetAttributeValue( AttributeKey.ExcludeCategories, box.Settings.ExcludeCategories.Select( a => a.Value ).ToList().AsDelimited( "," ) ) );

                block.SaveAttributeValues( rockContext );

                return ActionOk();
            }
        }

        #endregion
    }
}
