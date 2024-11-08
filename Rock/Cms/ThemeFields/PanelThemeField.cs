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
using System.Linq;

using Newtonsoft.Json.Linq;

using Rock.Enums.Cms;

namespace Rock.Cms.ThemeFields
{
    /// <summary>
    /// A UI field that wraps a set of items in a panel to indicate they
    /// are all related to each other.
    /// </summary>
    internal class PanelThemeField : ThemeField
    {
        /// <summary>
        /// The title of the panel.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Determines if the panel should be expanded by default.
        /// </summary>
        public bool IsExpanded { get; }

        /// <summary>
        /// The fields to be nested inside the panel.
        /// </summary>
        public List<ThemeField> Fields { get; }

        /// <summary>
        /// Creates a new instance of <see cref="PanelThemeField"/>.
        /// </summary>
        /// <param name="jField">The JSON object to be parsed.</param>
        /// <param name="type">The type of field of this instance.</param>
        public PanelThemeField( JObject jField, ThemeFieldType type )
            : base( type )
        {
            Name = jField.GetValue( "title" )?.ToString();
            IsExpanded = jField.GetValue( "expanded" )?.Value<bool>() ?? false;
            var jPanelFields = jField.GetValue( "fields" );

            if ( Name.IsNullOrWhiteSpace() )
            {
                throw new FormatException( "Panel field is missing 'title' property." );
            }

            Fields = ThemeDefinition.ParseFields( jPanelFields );

            if ( Fields.Any( f => f is PanelThemeField ) )
            {
                throw new FormatException( $"Panel '{Name}' may not have nested panels." );
            }
        }

        /// <inheritdoc/>
        public override void AddCssOverrides( IThemeOverrideBuilder builder )
        {
            Fields.ForEach( f => f.AddCssOverrides( builder ) );
        }
    }
}
