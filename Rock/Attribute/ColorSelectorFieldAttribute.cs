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
using Rock.Field.Types;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to set color 
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class ColorSelectorFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorSelectorFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="colors">The source of the hex color values to display. Format is "hexValue1|hexValue2|hexValue3|...".</param>
        /// <param name="defaultValue">The default hex color value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public ColorSelectorFieldAttribute( string name, string description = "", bool required = true, string colors = "", string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.ColorSelectorFieldType ).FullName )
        {
            Colors = colors;
        }

        /// <summary>
        /// The source of the hex color values to display. Format is "hexValue1|hexValue2|hexValue3|...".
        /// </summary>
        public string Colors
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( ColorSelectorFieldType.ConfigurationKey.Colors );
            }
            set
            {
                FieldConfigurationValues.AddOrReplace( ColorSelectorFieldType.ConfigurationKey.Colors, new Field.ConfigurationValue( value ) );
            }
        }
    }
}