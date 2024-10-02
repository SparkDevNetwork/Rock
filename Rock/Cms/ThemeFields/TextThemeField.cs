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

using Newtonsoft.Json.Linq;

using Rock.Enums.Cms;

namespace Rock.Cms.ThemeFields
{
    /// <summary>
    /// A theme field which represent a CSS variable that takes a string
    /// of text and outputs a variable with the text enclosed in quotes
    /// and escaped.
    /// </summary>
    internal class TextThemeField : VariableThemeField
    {
        /// <summary>
        /// Determines if the input field will be a multi-line input. A
        /// multi-line input will always be full width.
        /// </summary>
        public bool IsMultiline { get; set; }

        /// <summary>
        /// The width of the input. This will be appended to the string
        /// "input-width-" and set on the input as a CSS class.
        /// </summary>
        public string Width { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="TextThemeField"/>.
        /// </summary>
        /// <param name="jField">The JSON object to be parsed.</param>
        /// <param name="type">The type of field of this instance.</param>
        public TextThemeField( JObject jField, ThemeFieldType type )
            : base( jField, type )
        {
            IsMultiline = jField.GetValue( "multiline" )?.ToObject<bool>() ?? false;
            Width = jField.GetValue( "width" )?.ToString() ?? string.Empty;
        }

        /// <inheritdoc/>
        public override void AddCssOverrides( IThemeOverrideBuilder builder )
        {
            var rawValue = GetValueOrDefault( builder );

            // We are going to wrap the string in single quotes, so we need to
            // escape backslash, single quotes, and new line characters.
            var escapedValue = rawValue
                .Replace( "\\", "\\\\" )
                .Replace( "'", "\\'" )
                .Replace( "\n", "\\A" );

            builder.AddVariable( Variable, $"'{escapedValue}'" );
        }
    }
}
