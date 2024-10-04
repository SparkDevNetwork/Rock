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
using Rock.Cms;
using Rock.Cms.ThemeFields;
using Rock.Configuration;
using Rock.Constants;
using Rock.Data;
using Rock.Enums.Cms;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.ThemeDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular theme.
    /// </summary>

    [DisplayName( "Theme Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a particular theme." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "d4bfe2a3-b5fa-45cb-9c53-a6bea98ecdda" )]
    [Rock.SystemGuid.BlockTypeGuid( "4bd81377-e3c2-48c8-8bbe-20d2be915446" )]
    public class ThemeDetail : RockEntityDetailBlockType<Theme, ThemeBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string ThemeId = "ThemeId";
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
            var box = new DetailBlockBox<ThemeBag, ThemeDetailOptionsBag>();

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
        private ThemeDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new ThemeDetailOptionsBag
            {
                HasFontAwesomePro = FontAwesomeHelper.HasFontAwesomeProKey()
            };

            return options;
        }

        /// <summary>
        /// Validates the Theme for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="theme">The Theme to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Theme is valid, <c>false</c> otherwise.</returns>
        private bool ValidateTheme( Theme theme, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<ThemeBag, ThemeDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Theme.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Theme.FriendlyTypeName );
                }
            }
            else
            {
                // Creating themes this way is not currently supported.
                box.ErrorMessage = "Creating themes is not supported.";

                // New entity is being created, prepare for edit mode by default.
                //if ( box.IsEditable )
                //{
                //    box.Entity = GetEntityBagForEdit( entity );
                //}
                //else
                //{
                //    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Theme.FriendlyTypeName );
                //}
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="ThemeBag"/> that represents the entity.</returns>
        private ThemeBag GetCommonEntityBag( Theme entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var customization = entity.GetAdditionalSettingsOrNull<ThemeCustomizationSettings>();
            var themeDefinition = GetThemeDefinition( entity.Name );

            return new ThemeBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                IsSystem = entity.IsSystem,
                Name = entity.Name,
                Purpose = DefinedValueCache.Get( entity.PurposeValueId ?? 0, RockContext )?.Value ?? string.Empty,
                AvailableIconSets = themeDefinition.AvailableIconSets,
                EnabledIconSets = (customization?.EnabledIconSets ?? themeDefinition.AvailableIconSets) & themeDefinition.AvailableIconSets,
                DefaultFontAwesomeWeight = customization?.DefaultFontAwesomeWeight ?? ThemeFontAwesomeWeight.Solid,
                AdditionalFontAwesomeWeights = customization?.AdditionalFontAwesomeWeights ?? new List<ThemeFontAwesomeWeight>()
            };
        }

        /// <inheritdoc/>
        protected override ThemeBag GetEntityBagForView( Theme entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        //// <inheritdoc/>
        protected override ThemeBag GetEntityBagForEdit( Theme entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            var customization = entity.GetAdditionalSettingsOrNull<ThemeCustomizationSettings>();
            var themeDefinition = GetThemeDefinition( entity.Name );

            bag.VariableValues = customization?.VariableValues;
            bag.CustomOverrides = customization?.CustomOverrides;
            bag.Fields = themeDefinition.Fields.Select( f => GetFieldBag( f, entity.Name ) ).ToList();

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( Theme entity, ValidPropertiesBox<ThemeBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            var customization = entity.GetAdditionalSettings<ThemeCustomizationSettings>();
            var themeDefinition = GetThemeDefinition( entity.Name );

            box.IfValidProperty( nameof( box.Bag.VariableValues ), () =>
            {
                UpdateReferencedFiles( entity.Id, themeDefinition, customization.VariableValues, box.Bag.VariableValues );
                customization.VariableValues = box.Bag.VariableValues;
            } );

            box.IfValidProperty( nameof( box.Bag.CustomOverrides ),
                () => customization.CustomOverrides = box.Bag.CustomOverrides );

            box.IfValidProperty( nameof( box.Bag.EnabledIconSets ), () =>
            {
                // Filter out any values that aren't actually valid.
                var iconSets = box.Bag.EnabledIconSets & themeDefinition.AvailableIconSets;

                customization.EnabledIconSets = iconSets;
            } );

            if ( themeDefinition.AvailableIconSets.HasFlag( ThemeIconSet.FontAwesome ) )
            {
                box.IfValidProperty( nameof( box.Bag.DefaultFontAwesomeWeight ),
                    () => customization.DefaultFontAwesomeWeight = box.Bag.DefaultFontAwesomeWeight );

                box.IfValidProperty( nameof( box.Bag.AdditionalFontAwesomeWeights ),
                    () => customization.AdditionalFontAwesomeWeights = box.Bag.AdditionalFontAwesomeWeights );
            }

            entity.SetAdditionalSettings( customization );

            // Force an update so that the theme will recompile.
            entity.ModifiedDateTime = RockDateTime.Now;

            return true;
        }

        /// <summary>
        /// Update any referenced files. This will mark any old files for removal
        /// and mark any new files as persisted. This also updates RelatedEntity
        /// with records to link this theme to that file.
        /// </summary>
        /// <param name="themeId">The identifier of the theme.</param>
        /// <param name="themeDefinition">The theme definition used to find the fields.</param>
        /// <param name="originalValues">The original values before we update them.</param>
        /// <param name="newValues">The new values that are about to be saved.</param>
        private void UpdateReferencedFiles( int themeId, ThemeDefinition themeDefinition, Dictionary<string, string> originalValues, Dictionary<string, string> newValues )
        {
            var binaryFileService = new BinaryFileService( RockContext );
            var relatedEntityService = new RelatedEntityService( RockContext );

            var themeEntityTypeId = EntityTypeCache.Get<Theme>( true, RockContext ).Id;
            var binaryFileEntityTypeId = EntityTypeCache.Get<BinaryFile>( true, RockContext ).Id;

            var nestedFileOrImageFields = themeDefinition.Fields
                .Where( f => f is PanelThemeField )
                .Cast<PanelThemeField>()
                .SelectMany( f => f.Fields )
                .Where( f => f.Type == ThemeFieldType.Image || f.Type == ThemeFieldType.File );
            var fileOrImageFields = themeDefinition.Fields
                .Where( f => f.Type == ThemeFieldType.Image || f.Type == ThemeFieldType.File )
                .Union( nestedFileOrImageFields )
                .ToList();

            foreach ( var field in fileOrImageFields.Cast<VariableThemeField>() )
            {
                if ( originalValues?.TryGetValue( field.Variable, out var originalValue ) != true )
                {
                    originalValue = string.Empty;
                }

                if ( newValues?.TryGetValue( field.Variable, out var newValue ) != true )
                {
                    newValue = string.Empty;
                }

                var oldFileGuid = originalValue.AsGuidOrNull();
                var newFileGuid = newValue.AsGuidOrNull();

                // If the values are the same then we don't need to do anything.
                if ( oldFileGuid == newFileGuid )
                {
                    continue;
                }

                // If there was an old value, we need to mark it as temporary
                // so it will be cleaned up and also remove the reference to
                // the file.
                if ( oldFileGuid.HasValue )
                {
                    var oldBinaryFile = binaryFileService.Get( oldFileGuid.Value );

                    if ( oldBinaryFile != null )
                    {
                        oldBinaryFile.IsTemporary = true;

                        var oldRelatedEntities = relatedEntityService.Queryable()
                            .Where( r => r.SourceEntityTypeId == themeEntityTypeId
                                && r.SourceEntityId == themeId
                                && r.TargetEntityTypeId == binaryFileEntityTypeId
                                && r.TargetEntityId == oldBinaryFile.Id
                                && r.PurposeKey == "THEME-FILE" )
                            .ToList();

                        relatedEntityService.DeleteRange( oldRelatedEntities );
                    }
                }

                // If there was a new value, we need to mark it as not temporary
                // so it will be persisted and also add a reference to the file.
                if ( newFileGuid.HasValue )
                {
                    var newBinaryFile = binaryFileService.Get( newFileGuid.Value );

                    if ( newBinaryFile != null )
                    {
                        newBinaryFile.IsTemporary = false;

                        var newRelatedEntity = new RelatedEntity
                        {
                            SourceEntityTypeId = themeEntityTypeId,
                            SourceEntityId = themeId,
                            TargetEntityTypeId = binaryFileEntityTypeId,
                            TargetEntityId = newBinaryFile.Id,
                            PurposeKey = "THEME-FILE"
                        };

                        relatedEntityService.Add( newRelatedEntity );
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override Theme GetInitialEntity()
        {
            return GetInitialEntity<Theme, ThemeService>( RockContext, PageParameterKey.ThemeId );
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

        /// <summary>
        /// Convert a theme field to a bag that can be sent to the client.
        /// </summary>
        /// <param name="field">The field to be converted.</param>
        /// <param name="themeName">The name of the theme, used to translate <c>~~/</c> path prefixes.</param>
        /// <returns>A new instance of <see cref="ThemeFieldBag"/> that represents the field.</returns>
        private ThemeFieldBag GetFieldBag( ThemeField field, string themeName )
        {
            var fieldBag = new ThemeFieldBag
            {
                Type = field.Type
            };

            if ( field is VariableThemeField variableField )
            {
                fieldBag.Name = variableField.Name;
                fieldBag.Description = variableField.Description;
                fieldBag.Variable = variableField.Variable;

                if ( variableField.Type == ThemeFieldType.Image || variableField.Type == ThemeFieldType.File )
                {
                    fieldBag.DefaultValue = RockApp.Current.ResolveRockUrl( variableField.DefaultValue, themeName );
                }
                else
                {
                    fieldBag.DefaultValue = variableField.DefaultValue;
                }

                if ( field is TextThemeField textField )
                {
                    fieldBag.IsMultiline = textField.IsMultiline;
                    fieldBag.InputWidth = textField.Width;
                }
            }
            else if ( field is HeadingThemeField headingField )
            {
                fieldBag.Name = headingField.Name;
                fieldBag.Description = headingField.Description;
            }
            else if ( field is PanelThemeField panelField )
            {
                fieldBag.Name = panelField.Name;
                fieldBag.IsExpanded = panelField.IsExpanded;
                fieldBag.Fields = panelField.Fields.Select( pf => GetFieldBag( pf, themeName ) ).ToList();
            }

            return fieldBag;
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out Theme entity, out BlockActionResult error )
        {
            var entityService = new ThemeService( RockContext );
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
                // Creating new themes not supported in this manner.
                entity = null;

                // Create a new entity.
                //entity = new Theme();
                //entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Theme.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Theme.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Load the theme definition for the specified theme name.
        /// </summary>
        /// <param name="themeName">The name of the theme folder.</param>
        /// <returns>An instance of <see cref="ThemeDefinition"/>.</returns>
        private ThemeDefinition GetThemeDefinition( string themeName )
        {
            try
            {
                var webRoot = RockApp.Current.HostingSettings.WebRootPath;
                var jsonPath = Path.Combine( webRoot, "Themes", themeName, "theme.json" );
                var json = File.ReadAllText( jsonPath );

                return ThemeDefinition.Parse( json );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                throw new Exception( "Theme definition is invalid." );
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

            try
            {
                var bag = GetEntityBagForEdit( entity );

                return ActionOk( new ValidPropertiesBox<ThemeBag>
                {
                    Bag = bag,
                    ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
                } );
            }
            catch ( Exception ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<ThemeBag> box )
        {
            var entityService = new ThemeService( RockContext );

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
            if ( !ValidateTheme( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.ThemeId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<ThemeBag>
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
            var entityService = new ThemeService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            if ( !RockTheme.DeleteTheme( entity.Name, out errorMessage ) )
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
