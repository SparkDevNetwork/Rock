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

using Rock.Enums.Cms;

namespace Rock.Cms.ThemeFields
{
    /// <summary>
    /// A UI field that displays a heading above a set of fields.
    /// </summary>
    internal class HeadingThemeField : ThemeField
    {
        /// <summary>
        /// The title of the heading in the UI.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// An optional description to display below the title.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Creates a new instance of <see cref="HeadingThemeField"/>.
        /// </summary>
        /// <param name="jField">The JSON object to be parsed.</param>
        /// <param name="type">The type of field of this instance.</param>
        public HeadingThemeField( JObject jField, ThemeFieldType type )
            : base( type )
        {
            Name = jField.GetValue( "title" )?.ToString();
            Description = jField.GetValue( "description" )?.ToString() ?? string.Empty;

            if ( Name.IsNullOrWhiteSpace() )
            {
                throw new FormatException( "Heading field is missing 'title' property." );
            }
        }
    }
}
