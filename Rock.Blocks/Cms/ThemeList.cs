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
using System.IO;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.ThemeList;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of themes.
    /// </summary>

    [DisplayName( "Theme List" )]
    [Category( "CMS" )]
    [Description( "Lists themes in the Theme folder." )]
    [IconCssClass( "ti ti-photo-scan" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "Page to use for editing next-gen themes.",
        Key = AttributeKey.DetailPage )]

    [LinkedPage( "Theme Styler Page",
        Description = "Page to use for the theme styler page.",
        Key = AttributeKey.ThemeStylerPage )]

    [Rock.SystemGuid.EntityTypeGuid( "320E74C1-C3E1-4525-8E39-AD7690C488E9" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "C869BBD7-9376-4B66-B0DF-4617F69191D3" )]
    [Rock.SystemGuid.BlockTypeGuid( "FD99E0AA-E1CB-4049-A6F6-9C5F2A34F694" )]
    [CustomizedGrid]
    public class ThemeList : RockEntityListBlockType<Theme>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string ThemeStylerPage = "ThemeStylerPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
            public const string ThemeStylerPage = "ThemeStylerPage";
        }

        #endregion Keys

        #region Properties

        /// <inheritdoc/>
        protected override bool DisableAttributes => true;

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<ThemeListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private ThemeListOptionsBag GetBoxOptions()
        {
            return new ThemeListOptionsBag();
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "ThemeId", "((Key))" ),
                [NavigationUrlKey.ThemeStylerPage] = this.GetLinkedPageUrl( AttributeKey.ThemeStylerPage, "EditTheme", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Theme> GetListQueryable( RockContext rockContext )
        {
            return base.GetListQueryable( rockContext )
                .Where( t => t.Name != "RockOriginal" );
        }

        /// <inheritdoc/>
        protected override IQueryable<Theme> GetOrderedListQueryable( IQueryable<Theme> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( t => t.Name );
        }

        /// <inheritdoc/>
        protected override List<Theme> GetListItems( IQueryable<Theme> queryable, RockContext rockContext )
        {
            return base.GetListItems( queryable, rockContext )
                .DistinctBy( t => t.Name )
                .ToList();
        }

        /// <inheritdoc/>
        protected override GridDataBag GetGridDataBag( RockContext rockContext )
        {
            SyncThemesFromDisk();

            return base.GetGridDataBag( rockContext );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Theme> GetGridBuilder()
        {
            var legacyThemeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.THEME_PURPOSE_WEBSITE_LEGACY.AsGuid() );

            return new GridBuilder<Theme>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "description", a => a.Description )
                .AddTextField( "purpose", a => GetPurposeText( a.PurposeValueId ) )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "isSystem", a => a.IsSystem )
                .AddField( "allowsCompile", a => a.PurposeValueId.HasValue && a.PurposeValueId == legacyThemeValueId );
        }

        /// <summary>
        /// Gets the purpose defined-value text for a theme.
        /// </summary>
        /// <param name="purposeValueId">The purpose defined value identifier, if any.</param>
        /// <returns>The defined value text, or an empty string when the theme has no purpose.</returns>
        private static string GetPurposeText( int? purposeValueId )
        {
            if ( !purposeValueId.HasValue )
            {
                return string.Empty;
            }

            return DefinedValueCache.Get( purposeValueId.Value )?.Value ?? string.Empty;
        }

        /// <summary>
        /// Syncs database theme records to the folders on disk and removes the leftover RockOriginal folder.
        /// </summary>
        private void SyncThemesFromDisk()
        {
            DeleteRockOriginalTheme();

            var themeService = new ThemeService( RockContext );

            if ( themeService.UpdateThemes() )
            {
                RockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes the leftover RockOriginal theme folder if it is still on disk.
        /// </summary>
        private static void DeleteRockOriginalTheme()
        {
            var themeDirectory = RockApp.Current.MapPath( "~/Themes/RockOriginal" );

            if ( string.IsNullOrWhiteSpace( themeDirectory ) || !Directory.Exists( themeDirectory ) )
            {
                return;
            }

            try
            {
                Directory.Delete( themeDirectory, true );
            }
            catch
            {
                // Intentionally ignored: leftover-folder cleanup is best-effort and must not block the grid.
            }
        }

        /// <summary>
        /// Gets the theme identified by Id, IdKey, or Guid.
        /// </summary>
        /// <param name="key">The identifier of the theme.</param>
        /// <returns>The matching theme, or <c>null</c> if it was not found.</returns>
        private Theme GetTheme( string key )
        {
            return new ThemeService( RockContext ).Get( key, !PageCache.Layout.Site.DisablePredictableIds );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Deletes the specified theme from disk and then from the database.
        /// </summary>
        /// <param name="key">The identifier of the theme to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var themeService = new ThemeService( RockContext );
            var entity = GetTheme( key );

            if ( entity == null )
            {
                return ActionBadRequest( $"{Theme.FriendlyTypeName} not found." );
            }

            if ( entity.IsSystem )
            {
                return ActionBadRequest( "Cannot delete a system theme." );
            }

            var themesToDelete = themeService.Queryable()
                .Where( t => t.Name == entity.Name )
                .ToList();

            foreach ( var theme in themesToDelete )
            {
                if ( !themeService.CanDelete( theme, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }
            }

            var themeDirectory = RockApp.Current.MapPath( $"~/Themes/{entity.Name}" );

            if ( !string.IsNullOrWhiteSpace( themeDirectory ) && Directory.Exists( themeDirectory ) )
            {
                if ( !RockTheme.DeleteTheme( entity.Name, out var messages ) )
                {
                    return ActionBadRequest( messages );
                }
            }

            themeService.DeleteRange( themesToDelete );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Compiles the specified legacy theme.
        /// </summary>
        /// <param name="key">The identifier of the theme to compile.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Compile( string key )
        {
            var entity = GetTheme( key );

            if ( entity == null )
            {
                return ActionBadRequest( $"{Theme.FriendlyTypeName} not found." );
            }

            var legacyThemeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.THEME_PURPOSE_WEBSITE_LEGACY.AsGuid() );

            if ( !legacyThemeValueId.HasValue || entity.PurposeValueId != legacyThemeValueId.Value )
            {
                return ActionBadRequest( "This theme does not support compiling." );
            }

            var theme = new RockTheme( entity.Name );

            if ( !theme.Compile( out var messages ) )
            {
                return ActionBadRequest( messages );
            }

            return ActionOk();
        }

        /// <summary>
        /// Clones the specified theme to a new folder name and syncs the database.
        /// </summary>
        /// <param name="key">The identifier of the theme to clone.</param>
        /// <param name="newThemeName">The requested name of the cloned theme.</param>
        /// <returns>The cleaned folder name of the cloned theme when successful.</returns>
        [BlockAction]
        public BlockActionResult Clone( string key, string newThemeName )
        {
            var entity = GetTheme( key );

            if ( entity == null )
            {
                return ActionBadRequest( $"{Theme.FriendlyTypeName} not found." );
            }

            if ( newThemeName.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "New theme name is required." );
            }

            if ( !RockTheme.CloneTheme( entity.Name, newThemeName, out var messages ) )
            {
                return ActionBadRequest( messages );
            }

            var themeService = new ThemeService( RockContext );

            if ( themeService.UpdateThemes() )
            {
                RockContext.SaveChanges();
            }

            return ActionOk( RockTheme.CleanThemeName( newThemeName ) );
        }

        #endregion Block Actions
    }
}
