﻿// <copyright>
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
        /// The variables that we should include in the output.
        /// </summary>
        private readonly OrderedDictionary _variables = new OrderedDictionary();

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
                var sb = new StringBuilder();

                // Append the standard FontAwesome CSS file.
                var fontAwesomeUrl = RockApp.Current.ResolveRockUrl( "~/Styles/style-v2/icons/fontawesome-icon.css" );

                sb.AppendLine( $"@import url('{fontAwesomeUrl}');" );

                // Append each additional font weight file.
                if ( customization.AdditionalFontAwesomeWeights != null )
                {
                    foreach ( var weight in customization.AdditionalFontAwesomeWeights )
                    {
                        var weightName = weight.ToString().ToLower();
                        var fontUrl = RockApp.Current.ResolveRockUrl( $"~/Styles/style-v2/icons/fontawesome-{weightName}.css" );

                        sb.AppendLine( $"@import url('{fontUrl}');" );
                    }
                }

                // Append the default font weight file.
                var defaultFontWeight = customization.DefaultFontAwesomeWeight.ToString().ToLower();
                var defaultFontUrl = RockApp.Current.ResolveRockUrl( $"~/Styles/style-v2/icons/fontawesome-{defaultFontWeight}.css" );

                sb.AppendLine( $"@import url('{defaultFontUrl}');" );

                AddCustomContent( sb.ToString() );
            }

            if ( enabledIconSets.HasFlag( ThemeIconSet.Tabler ) )
            {
                var fontUrl = RockApp.Current.ResolveRockUrl( $"~/Styles/style-v2/icons/tabler-icon.css" );

                AddCustomContent( $"@import url('{fontUrl}');" );
            }
        }

        /// <inheritdoc/>
        public string Build()
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
            }

            foreach ( var content in _customContent )
            {
                sb.AppendLine();
                sb.AppendLine( content.Trim() );
            }

            return sb.ToString();
        }

        #endregion
    }
}
