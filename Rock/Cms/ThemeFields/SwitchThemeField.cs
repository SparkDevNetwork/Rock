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
        }

        /// <inheritdoc/>
        public override void AddCssOverrides( IThemeOverrideBuilder builder )
        {
            var rawValue = GetValueOrDefault( builder );

            if ( rawValue.AsBoolean() )
            {
                builder.AddVariable( Variable, OnValue );
                builder.AddCustomContent( OnContent );
            }
            else
            {
                builder.AddVariable( Variable, OffValue );
                builder.AddCustomContent( OffContent );
            }
        }
    }
}
