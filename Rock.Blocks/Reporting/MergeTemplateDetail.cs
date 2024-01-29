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
using System.IO;
using System.Linq;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.MergeTemplates;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Reporting.MergeTemplateDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Displays the details of a particular merge template.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Merge Template Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a particular merge template." )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [EnumField( "Merge Templates Ownership",
        Description = "Set this to restrict if the merge template must be a Personal or Global merge template. Note: If the user has EDIT authorization to this block, both Global and Personal templates can be edited regardless of this setting.",
        EnumSourceType = typeof( MergeTemplateOwnership ),
        IsRequired = true,
        DefaultValue = "Global",
        Key = AttributeKey.MergeTemplatesOwnership )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "3338d32f-20e0-4f6f-9abc-dd21558649c8" )]
    [Rock.SystemGuid.BlockTypeGuid( "b852db84-0cdf-4862-9ec7-cdbbbd5bb77a" )]
    public class MergeTemplateDetail : RockDetailBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string MergeTemplatesOwnership = "MergeTemplatesOwnership";
        }

        private static class PageParameterKey
        {
            public const string MergeTemplateId = "MergeTemplateId";
            public const string ParentCategoryId = "ParentCategoryId";
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
                var box = new DetailBlockBox<MergeTemplateBag, MergeTemplateDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );
                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<MergeTemplate>();
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
        private MergeTemplateDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new MergeTemplateDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the MergeTemplate for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="mergeTemplate">The MergeTemplate to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the MergeTemplate is valid, <c>false</c> otherwise.</returns>
        private bool ValidateMergeTemplate( MergeTemplate mergeTemplate, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            int personalMergeTemplateCategoryId = CategoryCache.Get( Rock.SystemGuid.Category.PERSONAL_MERGE_TEMPLATE.AsGuid() ).Id;
            if ( mergeTemplate.PersonAliasId.HasValue )
            {
                if ( mergeTemplate.CategoryId == 0 )
                {
                    // if the category picker isn't shown and/or the category isn't selected, and it's a personal filter...
                    mergeTemplate.CategoryId = personalMergeTemplateCategoryId;
                }

                // ensure Personal templates are only in the Personal merge template category
                if ( mergeTemplate.CategoryId != personalMergeTemplateCategoryId )
                {
                    errorMessage = "Personal Merge Templates must be in Personal category";
                    return false;
                }
            }
            else
            {
                if ( mergeTemplate.CategoryId == personalMergeTemplateCategoryId )
                {
                    // prohibit global templates from being in Personal category
                    errorMessage = "Person is required when using the Personal category";
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
        private void SetBoxInitialEntityState( DetailBlockBox<MergeTemplateBag, MergeTemplateDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {MergeTemplate.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized(Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized(Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( MergeTemplate.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( MergeTemplate.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="MergeTemplateBag"/> that represents the entity.</returns>
        private MergeTemplateBag GetCommonEntityBag( MergeTemplate entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var mergeTemplateOwnership = GetAttributeValue( AttributeKey.MergeTemplatesOwnership )
                .ConvertToEnum<Rock.Enums.Controls.MergeTemplateOwnership>( Rock.Enums.Controls.MergeTemplateOwnership.PersonalAndGlobal );

            return new MergeTemplateBag
            {
                IdKey = entity.IdKey,
                Category = entity.Category.ToListItemBag(),
                Description = entity.Description,
                MergeTemplateTypeEntityType = entity.MergeTemplateTypeEntityType.ToListItemBag(),
                Name = entity.Name,
                MergeTemplateOwnership = mergeTemplateOwnership,
                PersonAlias = mergeTemplateOwnership == Rock.Enums.Controls.MergeTemplateOwnership.Personal && entity.Id == 0 ? RequestContext.CurrentPerson.PrimaryAlias.ToListItemBag() : entity.PersonAlias.ToListItemBag(),
                TemplateBinaryFile = entity.TemplateBinaryFile.ToListItemBag()
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="MergeTemplateBag"/> that represents the entity.</returns>
        private MergeTemplateBag GetEntityBagForView( MergeTemplate entity )
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
        /// <returns>A <see cref="MergeTemplateBag"/> that represents the entity.</returns>
        private MergeTemplateBag GetEntityBagForEdit( MergeTemplate entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            var mergeTemplateOwnership = this.GetAttributeValue( AttributeKey.MergeTemplatesOwnership ).ConvertToEnum<MergeTemplateOwnership>( MergeTemplateOwnership.Global );

            if ( entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) || mergeTemplateOwnership == MergeTemplateOwnership.PersonalAndGlobal )
            {
                // If Authorized to EDIT, owner should be able to be changed, or converted to Global
                bag.ShowPersonPicker = true;
                bag.IsPersonRequired = false;
                bag.ShowCategoryPicker = true;
            }
            else if ( mergeTemplateOwnership == MergeTemplateOwnership.Global )
            {
                bag.ShowPersonPicker = true;
                bag.IsPersonRequired = false;
                bag.ShowCategoryPicker = true;
                bag.ExcludedCategoryIds = new List<string>()
                {
                    CategoryCache.Get( Rock.SystemGuid.Category.PERSONAL_MERGE_TEMPLATE.AsGuid() ).Guid.ToString()
                };
            }
            else if ( mergeTemplateOwnership == MergeTemplateOwnership.Personal )
            {
                // if ONLY personal merge templates are permitted, hide the category since it'll always be saved in the personal category
                bag.ShowCategoryPicker = false;

                // merge template should be only for the current person, so hide the person picker
                bag.ShowPersonPicker = false;

                // it is required, but not shown..
                bag.IsPersonRequired = false;
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
        private bool UpdateEntityFromBox( MergeTemplate entity, DetailBlockBox<MergeTemplateBag, MergeTemplateDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.Category ),
                () => entity.CategoryId = box.Entity.Category.GetEntityId<Category>( rockContext ) ?? 0 );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.MergeTemplateTypeEntityType ),
                () => entity.MergeTemplateTypeEntityTypeId = box.Entity.MergeTemplateTypeEntityType.GetEntityId<EntityType>( rockContext ) ?? 0);

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.PersonAlias ),
                () => entity.PersonAliasId = box.Entity.PersonAlias.GetEntityId<PersonAlias>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.TemplateBinaryFile ),
                () => entity.TemplateBinaryFileId = box.Entity.TemplateBinaryFile.GetEntityId<BinaryFile>( rockContext ) ?? 0);

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            if ( entity.CategoryId == 0 && entity.PersonAliasId.HasValue )
            {
                int personalMergeTemplateCategoryId = CategoryCache.Get( Rock.SystemGuid.Category.PERSONAL_MERGE_TEMPLATE.AsGuid() ).Id;

                // if the category picker isn't shown and/or the category isn't selected, and it's a personal filter...
                entity.CategoryId = personalMergeTemplateCategoryId;
            }

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="MergeTemplate"/> to be viewed or edited on the page.</returns>
        private MergeTemplate GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<MergeTemplate, MergeTemplateService>( rockContext, PageParameterKey.MergeTemplateId );
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
        private string GetSecurityGrantToken( MergeTemplate entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out MergeTemplate entity, out BlockActionResult error )
        {
            var entityService = new MergeTemplateService( rockContext );
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
                entity = new MergeTemplate();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{MergeTemplate.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized(Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${MergeTemplate.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the Name and best matching MergeTemplateType based on the selected file and check if the selected file type matches the MergeTemplateType 
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier</param>
        /// <param name="mergeTemplateTypeGuid">The <see cref="MergeTemplateType"/> guid</param>
        /// <param name="rockContext">The rock context</param>
        /// <returns></returns>
        private MergeTemplateFileValidationBag GetMergeTemplateValidationBag( int binaryFileId, Guid? mergeTemplateTypeGuid, RockContext rockContext )
        {
            var bag = new MergeTemplateFileValidationBag();
            var binaryFile = new BinaryFileService( rockContext ).Get( binaryFileId );
            var mergeTemplateEntityType = EntityTypeCache.Get( mergeTemplateTypeGuid ?? Guid.Empty );

            if ( binaryFile == null )
            {
                return bag;
            }

            bag.FileName = Path.GetFileNameWithoutExtension( binaryFile.FileName ).SplitCase().ReplaceWhileExists( "  ", " " );

            string fileExtension = Path.GetExtension( binaryFile.FileName ).TrimStart( '.' );
            if ( string.IsNullOrWhiteSpace( fileExtension ) )
            {
                // nothing more to do
                return bag;
            }

            MergeTemplateType mergeTemplateType = null;

            if ( mergeTemplateEntityType != null )
            {
                mergeTemplateType = MergeTemplateTypeContainer.GetComponent( mergeTemplateEntityType.Name );
            }
            else
            {
                foreach ( var templateType in MergeTemplateTypeContainer.Instance.Components.Values.Where( item => item.Value.IsActive ).Select( item => item.Value ) )
                {
                    if ( templateType.SupportedFileExtensions?.Any() == true && templateType.SupportedFileExtensions.Contains( fileExtension ) )
                    {
                        mergeTemplateType = templateType;
                        break;
                    }
                }
            }

            if ( mergeTemplateType == null )
            {
                bag.FileTypeWarningMessage = "Warning: Please select a template type.";
                return bag;
            }

            if ( !mergeTemplateType.SupportedFileExtensions.Contains( fileExtension ) )
            {
                bag.FileTypeWarningMessage = string.Format(
                    "Warning: The selected template type doesn't support '{0}' files. Please use a {1} file for this template type.",
                    fileExtension,
                    mergeTemplateType.SupportedFileExtensions.Select( a => a.Quoted() ).ToList().AsDelimited( ", ", " or " ) );
            }
            else
            {
                bag.MergeTemplateTypeEntityType = new ListItemBag() { Text = mergeTemplateType.EntityType.FriendlyName, Value = mergeTemplateType.EntityType.Guid.ToString() };
            }

            return bag;
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

                var box = new DetailBlockBox<MergeTemplateBag, MergeTemplateDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<MergeTemplateBag, MergeTemplateDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new MergeTemplateService( rockContext );

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
                if ( !ValidateMergeTemplate( entity, rockContext, out var validationMessage ) )
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
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.MergeTemplateId] = entity.IdKey
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
                var entityService = new MergeTemplateService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<MergeTemplateBag, MergeTemplateDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<MergeTemplateBag, MergeTemplateDetailOptionsBag>
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

        [BlockAction]
        public BlockActionResult ValidateFile( ListItemBag binaryFile, Guid? mergeTemplateTypeGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var binaryFileId = binaryFile.GetEntityId<BinaryFile>( rockContext ) ?? 0;
                var bag = GetMergeTemplateValidationBag( binaryFileId, mergeTemplateTypeGuid, rockContext );
                return ActionOk( bag );
            }
        }

        #endregion
    }
}
