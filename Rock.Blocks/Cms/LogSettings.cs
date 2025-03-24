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

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.Constants;
using Rock.Logging;
using Rock.Model;
using Rock.Security;
using Rock.SystemKey;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.LogSettings;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular exception log.
    /// </summary>
    [DisplayName( "Log Settings" )]
    [Category( "Administration" )]
    [Description( "Block to edit rock log settings." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [SystemGuid.EntityTypeGuid( "e5f272d4-e63f-46e7-9429-0d62cb458fd1" )]
    [SystemGuid.BlockTypeGuid( "fa01630c-18fb-472f-8bf1-013af257de3f" )]
    public class LogSettings : RockBlockType
    {
        #region Keys

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<LogSettingsBag, LogSettingsDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private LogSettingsDetailOptionsBag GetBoxOptions()
        {
            var options = new LogSettingsDetailOptionsBag()
            {
                VerbosityLevels = typeof( LogLevel ).ToEnumListItemBag(),
                StandardCategories = RockLogger.GetStandardCategories().ConvertAll( sc => new ListItemBag() { Text = sc, Value = sc } )
            };
            return options;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<LogSettingsBag, LogSettingsDetailOptionsBag> box )
        {
            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            // Existing entity was found, prepare for view mode by default.
            if ( isViewable )
            {
                box.Entity = GetCommonEntityBag();
            }
            else
            {
                box.ErrorMessage = EditModeMessage.NotAuthorizedToView( "Rock Log System Settings" );
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        private LogSettingsBag GetCommonEntityBag()
        {
            var rockConfig = GetInitialEntity();

            var bag = new LogSettingsBag()
            {
                AdvancedSettings = rockConfig.AdvancedSettings,
                IsLocalLoggingEnabled = rockConfig.IsLocalLoggingEnabled,
                IsObservabilityLoggingEnabled = rockConfig.IsObservabilityLoggingEnabled,
                MaxFileSize = rockConfig.MaxFileSize.ToString(),
                NumberOfLogFiles = rockConfig.NumberOfLogFiles.ToString(),
                StandardLogLevel = rockConfig.StandardLogLevel.ConvertToInt().ToString(),
            };

            var selectedCategories = rockConfig.StandardCategories ?? new List<string>();
            var standardCategories = RockLogger.GetStandardCategories();
            var categories = selectedCategories.Where( c => standardCategories.Contains( c ) ).ToList();
            var customCategories = selectedCategories.Where( c => !standardCategories.Contains( c ) ).ToList();

            bag.Categories = categories;
            bag.CustomCategories = customCategories;
            bag.SelectedCategories = selectedCategories;

            return bag;
        }

        /// <inheritdoc/>
        private bool UpdateEntityFromBox( RockLogSystemSettings entity, ValidPropertiesBox<LogSettingsBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.StandardLogLevel ),
                () => entity.StandardLogLevel = ( LogLevel ) box.Bag.StandardLogLevel.AsInteger() );

            box.IfValidProperty( nameof( box.Bag.Categories ),
                () => entity.StandardCategories = box.Bag.Categories.Union( box.Bag.CustomCategories ).Distinct().ToList() );

            box.IfValidProperty( nameof( box.Bag.IsLocalLoggingEnabled ),
                () => entity.IsLocalLoggingEnabled = box.Bag.IsLocalLoggingEnabled );

            box.IfValidProperty( nameof( box.Bag.IsObservabilityLoggingEnabled ),
                () => entity.IsObservabilityLoggingEnabled = box.Bag.IsObservabilityLoggingEnabled );

            box.IfValidProperty( nameof( box.Bag.AdvancedSettings ),
                () => entity.AdvancedSettings = box.Bag.AdvancedSettings );

            box.IfValidProperty( nameof( box.Bag.MaxFileSize ),
                () => entity.MaxFileSize = box.Bag.MaxFileSize.AsInteger() );

            box.IfValidProperty( nameof( box.Bag.NumberOfLogFiles ),
                () => entity.NumberOfLogFiles = box.Bag.NumberOfLogFiles.AsInteger() );

            return true;
        }

        /// <inheritdoc/>
        private RockLogSystemSettings GetInitialEntity()
        {
            return Rock.Web.SystemSettings.GetValue( SystemSetting.ROCK_LOGGING_SETTINGS ).FromJsonOrNull<RockLogSystemSettings>() ?? new RockLogSystemSettings();
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
        private bool TryGetEntityForEditAction( out RockLogSystemSettings entity, out BlockActionResult error )
        {
            error = null;
            entity = GetInitialEntity();

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( "Not authorized to edit Rock Log System Settings." );
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
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit()
        {
            if ( !TryGetEntityForEditAction( out var entity, out var actionError ) )
            {
                return actionError;
            }

            var bag = GetCommonEntityBag();

            return ActionOk( new ValidPropertiesBox<LogSettingsBag>
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
        public BlockActionResult Save( ValidPropertiesBox<LogSettingsBag> box )
        {
            if ( !TryGetEntityForEditAction( out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            Rock.Web.SystemSettings.SetValue( SystemSetting.ROCK_LOGGING_SETTINGS, entity.ToJson() );

            var bag = GetCommonEntityBag();

            return ActionOk( new ValidPropertiesBox<LogSettingsBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete()
        {
            RockLogger.RecycleSerilog();
            ( RockLogger.LogReader as RockSerilogReader )?.Delete();
            return ActionOk( this.GetCurrentPageUrl() );
        }

        #endregion
    }
}
