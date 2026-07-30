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
    /// Field attribute for selecting a Date.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class DateFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <param name="allowCurrentOption">if set to <c>true</c> [allow current option].</param>
        [Obsolete( "Use the constructor that takes a name only." )]
        [RockObsolete( "20.0" )]
        public DateFieldAttribute( string name, string description = "", bool required = true, string defaultValue = "", string category = "",
            int order = 0, string key = null, bool allowCurrentOption = false )
            : base( SystemGuid.FieldType.DATE.AsGuid(), name, description, required, defaultValue, category, order, key )
        {
            var displayCurrentConfigValue = new Field.ConfigurationValue( allowCurrentOption.ToString() );
            FieldConfigurationValues.AddOrReplace( "displayCurrentOption", displayCurrentConfigValue );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public DateFieldAttribute( string name )
            : base( SystemGuid.FieldType.DATE.AsGuid(), name )
        {
            IsCurrentAllowed = false;
        }

        /// <summary>
        /// Determines if the date field should include an option to select
        /// the current date.
        /// </summary>
        public bool IsCurrentAllowed
        {
            get => FieldConfigurationValues.GetValueOrNull( "displayCurrentOption" ).AsBoolean();
            set => FieldConfigurationValues.AddOrReplace( "displayCurrentOption", new Field.ConfigurationValue( value.ToString() ) );
        }
    }
}
