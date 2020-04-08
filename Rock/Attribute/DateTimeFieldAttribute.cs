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
    /// Field attribute for selecting a Date Time.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class DateTimeFieldAttribute : FieldAttribute
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
        public DateTimeFieldAttribute( string name, string description = "", bool required = true, string defaultValue = "", string category = "",
            int order = 0, string key = null, bool allowCurrentOption = false )
            : base( name, description, required, defaultValue, category, order, key, typeof( DateTimeFieldType ).FullName )
        {
            var displayCurrentConfigValue = new Field.ConfigurationValue( allowCurrentOption.ToString() );
            FieldConfigurationValues.Add( "displayCurrentOption", displayCurrentConfigValue );
        }
    }
}
