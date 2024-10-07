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

using Newtonsoft.Json.Linq;

using Rock.Configuration;
using Rock.Enums.Cms;

namespace Rock.Cms.ThemeFields
{
    /// <summary>
    /// A theme field which represent a CSS variable that can be edited.
    /// </summary>
    internal abstract class VariableThemeField : ThemeField
    {
        /// <summary>
        /// The friendly name of the field.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The CSS variable name, this should not include the <c>--</c> prefix.
        /// </summary>
        public string Variable { get; }

        /// <summary>
        /// The help text to describe how this variable is used. This will
        /// never be <c>null</c>.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The default value of this field if not set in the overrides.
        /// This will never be <c>null</c>.
        /// </summary>
        public string DefaultValue { get; }

        /// <summary>
        /// Creates a new instance of <see cref="VariableThemeField"/>.
        /// </summary>
        /// <param name="jField">The JSON object to be parsed.</param>
        /// <param name="type">The type of field of this instance.</param>
        protected VariableThemeField( JObject jField, ThemeFieldType type )
            : base( type )
        {
            Name = jField.GetValue( "name" )?.ToString();
            Variable = jField.GetValue( "variable" )?.ToString();
            Description = jField.GetValue( "description" )?.ToString() ?? string.Empty;
            DefaultValue = jField.GetValue( "default" )?.ToString() ?? string.Empty;

            if ( Name.IsNullOrWhiteSpace() )
            {
                throw new FormatException( $"{type} field is missing 'name' property." );
            }

            if ( Variable.IsNullOrWhiteSpace() )
            {
                throw new FormatException( $"{type} field is missing 'variable' property." );
            }
        }

        /// <summary>
        /// Get the raw value entered in the UI or the default value if one was
        /// not set in the UI.
        /// </summary>
        /// <param name="builder">The builder to get the variable values from.</param>
        /// <returns>A string that contains the value.</returns>
        protected string GetValueOrDefault( IThemeOverrideBuilder builder )
        {
            if ( builder.VariableValues.TryGetValue( Variable, out var rawValue ) )
            {
                return rawValue ?? DefaultValue;
            }

            return DefaultValue;
        }
    }
}
