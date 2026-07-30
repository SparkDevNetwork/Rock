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

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to set color 
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class ColorFieldAttribute : FieldAttribute
    {
        private const string SELECTION_TYPE = "selectiontype";

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="selctionType">The selection type.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        [Obsolete( "Use the constructor that takes a name only." )]
        [RockObsolete( "20.0" )]
        public ColorFieldAttribute( string name, string description = "", bool required = true, string selctionType = "Color Picker", string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( SystemGuid.FieldType.COLOR.AsGuid(), name, description, required, defaultValue, category, order, key )
        {
            SelectionType = selctionType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ColorFieldAttribute( string name )
            : base( SystemGuid.FieldType.COLOR.AsGuid(), name )
        {
            SelectionType = "Color Picker";
        }

        /// <summary>
        /// The type of selection control to use. Options are "Color Picker"
        /// or "Named Color".
        /// </summary>
        public string SelectionType
        {
            get => FieldConfigurationValues.GetValueOrNull( SELECTION_TYPE );
            set => FieldConfigurationValues.AddOrReplace( SELECTION_TYPE, new Field.ConfigurationValue( value ) );
        }
    }
}
