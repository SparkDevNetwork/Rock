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
using System.IO;
using System.Linq;
using System.Threading;

using Rock.Attribute;
using Rock.Cms;
using Rock.Configuration;
using Rock.Data;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for the <see cref="Rock.Model.Theme"/> entity. This inherits from the Service class
    /// </summary>
    public partial class ThemeService
    {
        /// <summary>
        /// Update the themes in the database to match what is on disk. This
        /// will make any required changes on the database context but will
        /// not save those changes. The caller must save the changes.
        /// </summary>
        /// <returns><c>true</c> if any database changes were made; otherwise <c>false</c>.</returns>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.7", true )]
        public bool UpdateThemes()
        {
            bool anyThemesUpdated = false;
            var themes = RockTheme.GetThemes();

            if ( themes == null || !themes.Any() )
            {
                return false;
            }

            var dbThemes = Queryable().ToList();
            var websiteLegacyValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.THEME_PURPOSE_WEBSITE_LEGACY.AsGuid() );
            var websiteValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.THEME_PURPOSE_WEBSITE_NEXTGEN.AsGuid() );
            var checkinValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.THEME_PURPOSE_CHECKIN.AsGuid() );

            // Create new database themes for any themes on disk that don't
            // exist in the database yet.
            foreach ( var theme in themes.Where( a => !dbThemes.Any( b => b.Name == a.Name ) ) )
            {
                var dbTheme = new Theme
                {
                    Name = theme.Name,
                    IsSystem = theme.IsSystem,
                    RootPath = theme.RelativePath,
                    PurposeValueId = websiteLegacyValueId
                };
                Add( dbTheme );
                anyThemesUpdated = true;
            }

            // Mark any database themes that no longer exist on disk as
            // inactive.
            foreach ( var dbTheme in dbThemes.Where( a => !themes.Any( b => b.Name == a.Name ) ) )
            {
                dbTheme.IsActive = false;
                anyThemesUpdated = true;
            }

            // Update any themes that exist in both the database and on disk to
            // match what is on disk.
            foreach ( var dbTheme in dbThemes )
            {
                var theme = themes.FirstOrDefault( a => a.Name == dbTheme.Name );

                if ( theme == null )
                {
                    continue;
                }

                if ( !dbTheme.IsActive )
                {
                    dbTheme.IsActive = true;
                    anyThemesUpdated = true;
                }

                if ( dbTheme.IsSystem != theme.IsSystem )
                {
                    dbTheme.IsSystem = theme.IsSystem;
                    anyThemesUpdated = true;
                }

                var themeJsonFile = Path.Combine( RockApp.Current.HostingSettings.WebRootPath, $"Themes/{dbTheme.Name}/theme.json" );

                if ( !File.Exists( themeJsonFile ) )
                {
                    continue;
                }

                var themeJson = File.ReadAllText( themeJsonFile );
                if ( !ThemeDefinition.TryParse( themeJson, out var themeDefinition ) )
                {
                    continue;
                }

                if ( dbTheme.Description != themeDefinition.Description )
                {
                    dbTheme.Description = themeDefinition.Description;
                    anyThemesUpdated = true;
                }

                if ( themeDefinition.Purpose == ThemePurpose.Web && dbTheme.PurposeValueId != websiteValueId )
                {
                    dbTheme.PurposeValueId = websiteValueId;
                    anyThemesUpdated = true;
                }
                else if ( themeDefinition.Purpose == ThemePurpose.Checkin && dbTheme.PurposeValueId != checkinValueId )
                {
                    dbTheme.PurposeValueId = checkinValueId;
                    anyThemesUpdated = true;
                }
            }

            return anyThemesUpdated;
        }

        /// <summary>
        /// Compiles all next-generation themes. This finds all themes in the
        /// database that are next-generation themes and then calls
        /// <see cref="BuildTheme(Theme)"/> for each one.
        /// </summary>
        /// <param name="cancellationToken">A token that indicates if the compile process should be aborted.</param>
        /// <returns>A list of any messages that resulted from the compile.</returns>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.7", true )]
        public static List<string> CompileAll( CancellationToken cancellationToken )
        {
            using ( var rockContext = new RockContext() )
            {
                var themes = new ThemeService( rockContext ).Queryable().ToList();
                var legacyPurposeId = DefinedValueCache.Get( SystemGuid.DefinedValue.THEME_PURPOSE_WEBSITE_LEGACY.AsGuid(), rockContext ).Id;
                var messages = new List<string>();

                foreach ( var theme in themes )
                {
                    if ( theme.PurposeValueId == null || theme.PurposeValueId == legacyPurposeId )
                    {
                        continue;
                    }

                    try
                    {
                        BuildTheme( theme );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex );

                        messages.Add( ex.Message );
                    }
                }

                return messages;
            }
        }

        /// <summary>
        /// Build the theme specified so that the files on disk match the
        /// current database configuration values. This currently writes out
        /// the CSS overrides file based on the database configuration. In the
        /// future it might write other changes to disk.
        /// </summary>
        /// <param name="themeId">The identifier of the theme to build.</param>
        internal static void BuildTheme( int themeId )
        {
            using ( var rockContext = new RockContext() )
            {
                var theme = new ThemeService( rockContext ).Get( themeId );

                BuildTheme( theme );
            }
        }

        /// <summary>
        /// Build the theme specified so that the files on disk match the
        /// current database configuration values. This currently writes out
        /// the CSS overrides file based on the database configuration. In the
        /// future it might write other changes to disk.
        /// </summary>
        /// <param name="theme">The theme to build.</param>
        internal static void BuildTheme( Theme theme )
        {
            var customization = theme.GetAdditionalSettings<ThemeCustomizationSettings>();

            var webRoot = RockApp.Current.HostingSettings.WebRootPath;
            var themePath = Path.Combine( webRoot, "Themes", theme.Name );
            var stylesPath = Path.Combine( themePath, "Styles" );
            var jsonPath = Path.Combine( themePath, "theme.json" );
            var themeCssPath = Path.Combine( stylesPath, "theme.css" );

            var json = File.ReadAllText( jsonPath );
            var themeDefinition = ThemeDefinition.Parse( json );
            var cssContent = string.Empty;

            if ( !Directory.Exists( stylesPath ) )
            {
                Directory.CreateDirectory( stylesPath );
            }

            if ( File.Exists( themeCssPath ) )
            {
                cssContent = File.ReadAllText( themeCssPath );
            }

            cssContent = GetThemeCssContent( themeDefinition, customization, theme.Name, cssContent );

            File.WriteAllText( themeCssPath, cssContent );
        }

        /// <summary>
        /// Gets the content of the theme.css file that match the original
        /// content with the customized settings of the theme applied.
        /// </summary>
        /// <param name="themeDefinition">This describes the theme and how the overrides should be structured.</param>
        /// <param name="customization">The customization settings that were set by an administrator.</param>
        /// <param name="themeName">The name of the theme, this is used to resolve <c>~~/</c> image references.</param>
        /// <param name="originalCssContent">The original content of the theme.css file.</param>
        /// <returns>The content that should be written to the theme.css file.</returns>
        internal static string GetThemeCssContent( ThemeDefinition themeDefinition, ThemeCustomizationSettings customization, string themeName, string originalCssContent )
        {
            var builder = new ThemeOverrideBuilder( themeName, customization.VariableValues );

            themeDefinition.Fields.ForEach( f => f.AddCssOverrides( builder ) );

            builder.AddFontIconSets( themeDefinition, customization );

            // Append the custom CSS override content provided by the person.
            if ( customization.CustomOverrides.IsNotNullOrWhiteSpace() )
            {
                builder.AddCustomContent( customization.CustomOverrides );
            }

            return builder.Build( originalCssContent );
        }
    }
}
