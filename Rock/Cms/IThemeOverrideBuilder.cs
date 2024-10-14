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

namespace Rock.Cms
{
    /// <summary>
    /// Builds a CSS overrides file for a theme.
    /// </summary>
    internal interface IThemeOverrideBuilder
    {
        /// <summary>
        /// The name of the theme being built. This helps provide context when
        /// creating URLs for things inside the theme folder.
        /// </summary>
        string ThemeName { get; }

        /// <summary>
        /// The raw values that have been entered in the theme editor UI.
        /// </summary>
        IReadOnlyDictionary<string, string> VariableValues { get; }

        /// <summary>
        /// Add a variable to the CSS output. If another variable with the same
        /// name has already been added then this will overwrite the previous
        /// value.
        /// </summary>
        /// <param name="name">The name of the CSS variable, not including the <c>--</c> prefix.</param>
        /// <param name="value">The value of the variable, not including the trailing <c>;</c>.</param>
        void AddVariable( string name, string value );

        /// <summary>
        /// <para>
        /// Adds a URL to be imported into the overrides file. This should be
        /// a bare URL and will have any leading ~ or ~~ expanded automatically.
        /// </para>
        /// <para>
        /// Example: <example>AddImport( "~~/Styles/on.css" );</example>
        /// </para>
        /// </summary>
        /// <param name="url">The URL to be imported.</param>
        void AddImport( string url );

        /// <summary>
        /// Add a string of custom content to the overrides file. The content
        /// will be emitted as-is into the file after all the variables have
        /// been defined.
        /// </summary>
        /// <param name="content">The raw custom CSS content.</param>
        void AddCustomContent( string content );

        /// <summary>
        /// <para>
        /// Builds the CSS file from all the data provided to the builder.
        /// </para>
        /// <para>
        /// This will take an existing theme.css and insert override content
        /// at the top and bottom, replacing any existing override content.
        /// </para>
        /// </summary>
        /// <param name="originalThemeCss">The content of the theme.css file to be updated.</param>
        /// <returns>The content of the CSS file.</returns>
        string Build( string originalThemeCss );
    }
}
