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
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Rock.Configuration;
using Rock.Enums.Cms;

namespace Rock.Cms
{
    /// <summary>
    /// Builds a CSS overrides file for a theme.
    /// </summary>
    internal class ThemeOverrideBuilder : IThemeOverrideBuilder
    {
        #region Fields

        /// <summary>
        /// The marker used to indicate the start of the top overrides section.
        /// </summary>
        internal const string TopOverrideStartMarker = "/* CSS Overrides Top Start */";

        /// <summary>
        /// The marker used to indicate the end of the top overrides section.
        /// </summary>
        internal const string TopOverrideEndMarker = "/* CSS Overrides Top End */";

        /// <summary>
        /// The marker used to indicate the start of the bottom overrides section.
        /// </summary>
        internal const string BottomOverrideStartMarker = "/* CSS Overrides Bottom Start */";

        /// <summary>
        /// The marker used to indicate the end of the bottom overrides section.
        /// </summary>
        internal const string BottomOverrideEndMarker = "/* CSS Overrides Bottom End */";

        /// <summary>
        /// A regular expression that finds all the top override section content
        /// including the start and end markers.
        /// </summary>
        private static readonly Regex TopOverridePattern = new Regex( $"{TopOverrideStartMarker.Replace( "/", "\\/" ).Replace( "*", "\\*" )}.*{TopOverrideEndMarker.Replace( "/", "\\/" ).Replace( "*", "\\*" )}", RegexOptions.Compiled | RegexOptions.Singleline );

        /// <summary>
        /// A regular expression that finds all the bottom override section content
        /// including the start and end markers.
        /// </summary>
        private static readonly Regex BottomOverridePattern = new Regex( $"{BottomOverrideStartMarker.Replace( "/", "\\/" ).Replace( "*", "\\*" )}.*{BottomOverrideEndMarker.Replace( "/", "\\/" ).Replace( "*", "\\*" )}", RegexOptions.Compiled | RegexOptions.Singleline );

        /// <summary>
        /// The variables that we should include in the output.
        /// </summary>
        private readonly OrderedDictionary _variables = new OrderedDictionary();

        /// <summary>
        /// The list of CSS files to be imported into the output.
        /// </summary>
        private readonly List<string> _urlImports = new List<string>();

        /// <summary>
        /// Any strings of custom content that should be included after all
        /// the variables have been defined.
        /// </summary>
        private readonly List<string> _customContent = new List<string>();

        #endregion

        #region Properties

        /// <inheritdoc/>
        public string ThemeName { get; }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, string> VariableValues { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="ThemeOverrideBuilder"/> that
        /// can be used to build an <c>overrides.css</c> file.
        /// </summary>
        /// <param name="themeName">The name of the theme. This will be used when resolving URL references to the theme folder.</param>
        /// <param name="variableValues">The raw variable values from the UI.</param>
        internal ThemeOverrideBuilder( string themeName, IReadOnlyDictionary<string, string> variableValues )
        {
            ThemeName = themeName;
            VariableValues = variableValues ?? new Dictionary<string, string>();
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void AddVariable( string name, string value )
        {
            _variables[name] = value;
        }

        /// <inheritdoc/>
        public void AddImport( string url )
        {
            if ( url.IsNotNullOrWhiteSpace() )
            {
                _urlImports.Add( url );
            }
        }

        /// <inheritdoc/>
        public void AddCustomContent( string content )
        {
            if ( content.IsNotNullOrWhiteSpace() )
            {
                _customContent.Add( content );
            }
        }

        /// <summary>
        /// Adds the font icon sets that are configured for the theme.
        /// </summary>
        /// <param name="themeDefinition">The definition of what the theme supports.</param>
        /// <param name="customization">The customization options that describe what is enabled.</param>
        internal void AddFontIconSets( ThemeDefinition themeDefinition, ThemeCustomizationSettings customization )
        {
            // Get the icon sets enabled for the theme. If nothing has been
            // configured yet then enable everything. Also make sure the theme
            // still supports the selection.
            var enabledIconSets = ( customization.EnabledIconSets ?? themeDefinition.AvailableIconSets )
                & themeDefinition.AvailableIconSets;

            if ( enabledIconSets.HasFlag( ThemeIconSet.FontAwesome ) )
            {
                // Append the standard FontAwesome CSS file.
                AddImport( "~/Styles/style-v2/icons/fontawesome-icon.css" );

                // Append each additional font weight file.
                if ( customization.AdditionalFontAwesomeWeights != null )
                {
                    foreach ( var weight in customization.AdditionalFontAwesomeWeights )
                    {
                        var weightName = weight.ToString().ToLower();

                        AddImport( $"~/Styles/style-v2/icons/fontawesome-{weightName}.css" );
                    }
                }

                // Append the default font weight file.
                var defaultFontWeight = customization.DefaultFontAwesomeWeight.ToString().ToLower();

                AddImport( $"~/Styles/style-v2/icons/fontawesome-{defaultFontWeight}.css" );
            }

            if ( enabledIconSets.HasFlag( ThemeIconSet.Tabler ) )
            {
                AddImport( $"~/Styles/style-v2/icons/tabler-icon.css" );
            }
        }

        /// <inheritdoc/>
        public string Build( string originalThemeCss )
        {
            var sb = new StringBuilder();

            originalThemeCss = TopOverridePattern.Replace( originalThemeCss, string.Empty );
            originalThemeCss = BottomOverridePattern.Replace( originalThemeCss, string.Empty );
            originalThemeCss = originalThemeCss.Trim();

            var topOverrides = BuildTopOverrides().Trim();
            var bottomOverrides = BuildBottomOverrides().Trim();

            if ( topOverrides.Length > 0 )
            {
                sb.AppendLine( TopOverrideStartMarker );
                sb.AppendLine( topOverrides );
                sb.AppendLine( TopOverrideEndMarker );
                sb.AppendLine();
            }

            if ( originalThemeCss.Length > 0 )
            {
                sb.AppendLine( originalThemeCss );
            }

            if ( bottomOverrides.Length > 0 )
            {
                sb.AppendLine();
                sb.AppendLine( BottomOverrideStartMarker );
                sb.AppendLine( bottomOverrides );
                sb.AppendLine( BottomOverrideEndMarker );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds the overrides that need to go at the top of the theme.css
        /// file.
        /// </summary>
        /// <returns>A string that contains the top overrides content.</returns>
        private string BuildTopOverrides()
        {
            var sb = new StringBuilder();

            if ( _urlImports.Any() )
            {
                foreach ( var importUrl in _urlImports )
                {
                    var url = importUrl.StartsWith( "~" )
                        ? RockApp.Current.ResolveRockUrl( importUrl, ThemeName )
                        : importUrl;

                    sb.AppendLine( $"@import url('{url}');" );
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds the overrides that need to go at the bottom of the theme.css
        /// file.
        /// </summary>
        /// <returns>A string that contains the bottom overrides content.</returns>
        private string BuildBottomOverrides()
        {
            var sb = new StringBuilder();

            if ( _variables.Values.Cast<string>().Any( v => v.IsNotNullOrWhiteSpace() ) )
            {
                sb.AppendLine( ":root {" );

                foreach ( var key in _variables.Keys )
                {
                    var value = ( string ) _variables[key];

                    if ( !value.IsNullOrWhiteSpace() )
                    {
                        sb.AppendLine( $"    --{key}: {_variables[key]};" );
                    }
                }

                sb.AppendLine( "}" );
                sb.AppendLine();
            }

            foreach ( var content in _customContent )
            {
                sb.AppendLine( content.Trim() );
                sb.AppendLine();
            }

            return sb.ToString();
        }

        #endregion
    }
}
