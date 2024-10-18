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

using Newtonsoft.Json.Linq;

using Rock.Enums.Cms;

namespace Rock.Cms.ThemeFields
{
    /// <summary>
    /// A theme field which represent a CSS variable and custom content that
    /// will include one of two values depending on the state.
    /// </summary>
    internal class SwitchThemeField : VariableThemeField
    {
        #region Properties

        /// <summary>
        /// The CSS content to include when the value of the switch field is on.
        /// </summary>
        public string OnContent { get; set; }

        /// <summary>
        /// The CSS value to use when the switch field is on.
        /// </summary>
        public string OnValue { get; set; }

        /// <summary>
        /// The CSS content to include when the value of the switch field is off.
        /// </summary>
        public string OffContent { get; set; }

        /// <summary>
        /// The CSS value to use when the switch field is off.
        /// </summary>
        public string OffValue { get; set; }

        /// <summary>
        /// The URLs that should be added as imports when the switch field is on.
        /// These may start with ~ or ~~ and will be resolved by Rock.
        /// </summary>
        public List<string> OnImports { get; set; }

        /// <summary>
        /// The URLs that should be added as imports when the switch field is off.
        /// These may start with ~ or ~~ and will be resolved by Rock.
        /// </summary>
        public List<string> OffImports { get; set; }

        #endregion

        /// <summary>
        /// Creates a new instance of <see cref="SwitchThemeField"/>.
        /// </summary>
        /// <param name="jField">The JSON object to be parsed.</param>
        /// <param name="type">The type of field of this instance.</param>
        public SwitchThemeField( JObject jField, ThemeFieldType type )
            : base( jField, type )
        {
            OnContent = jField.GetValue( "onContent" )?.ToString() ?? string.Empty;
            OffContent = jField.GetValue( "offContent" )?.ToString() ?? string.Empty;
            OnValue = jField.GetValue( "onValue" )?.ToString() ?? string.Empty;
            OffValue = jField.GetValue( "offValue" )?.ToString() ?? string.Empty;
            OnImports = ParseImports( jField.GetValue( "onImport" ) );
            OffImports = ParseImports( jField.GetValue( "offImport" ) );
        }

        /// <summary>
        /// Gets the CSS Import URLs from the JSON token. This must be either
        /// a string or an array of strings otherwise an exception will be
        /// thrown.
        /// </summary>
        /// <param name="jValue">The JSON token value to parse.</param>
        /// <returns>A list of strings that represents the URL(s).</returns>
        private List<string> ParseImports( JToken jValue )
        {
            if ( jValue == null )
            {
                return new List<string>();
            }

            if ( jValue.Type == JTokenType.Array)
            {
                var list = new List<string>();

                foreach ( var jItem in ( JArray ) jValue )
                {
                    if ( jItem.Type == JTokenType.String )
                    {
                        list.Add( jItem.ToString() );
                    }
                    else
                    {
                        throw new FormatException( "Import URLs must be of type string or an array of strings." );
                    }
                }

                return list;
            }
            else if ( jValue.Type == JTokenType.String )
            {
                return new List<string>
                {
                    jValue.ToString()
                };
            }
            else
            {
                throw new FormatException( "Import URLs must be of type string or an array of strings." );
            }
        }

        /// <inheritdoc/>
        public override void AddCssOverrides( IThemeOverrideBuilder builder )
        {
            var rawValue = GetValueOrDefault( builder );

            if ( rawValue.AsBoolean() )
            {
                builder.AddVariable( Variable, OnValue );
                builder.AddCustomContent( OnContent );

                foreach ( var url in OnImports )
                {
                    builder.AddImport( url );
                }
            }
            else
            {
                builder.AddVariable( Variable, OffValue );
                builder.AddCustomContent( OffContent );


                foreach ( var url in OffImports )
                {
                    builder.AddImport( url );
                }
            }
        }
    }
}
